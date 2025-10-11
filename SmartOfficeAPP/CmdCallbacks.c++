#include <SPI.h>
#include <MFRC522.h>
#include <ESP32Servo.h>
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>
#include <mbedtls/md.h>
#include <mbedtls/base64.h>
#include <vector>
#include <WiFiManager.h> // tzapu/WiFiManager
#include <NimBLEDevice.h> // BLE léger

// ===== Sélection des modes =====
#define USE_WIFI_MANAGER   1
#define USE_INSECURE_TLS   1

// ===== Pins matérielles =====
#define RST_PIN 4
#define SS_PIN  5
#define SERVO_PIN 13
#define LED_ETAGE3 14
#define LED_RDC 27
#define LED_ETAGE2 26
#define BUZZER_PIN 25
#define SMOKE_SENSOR_PIN 32
#define WIFI_CONFIG_BUTTON_PIN 0

// ===== Options =====
#define BYPASS_SMOKE_FOR_RF_TEST 0
#define SPI_FREQ_HZ 1000000 // 1 MHz: stable et rapide

// ===== CONFIG AZURE =====
#if !USE_WIFI_MANAGER
const char* WIFI_SSID     = "VotreSSID";
const char* WIFI_PASSWORD = "VotrePass";
#endif
const char* IOTHUB_HOST   = "brainoffice.azure-devices.net";
const char* DEVICE_ID     = "mon-esp32";
const char* DEVICE_KEY_B64 = "r0XMhfEf9ZdVXgVTLdL+CZcfY6xeEkFldjY+FOG3xe8=";
const unsigned long TELEMETRY_MS = 10000;
const uint32_t SAS_TTL_SEC = 3600;

#if !USE_INSECURE_TLS
// Remplissez si vous utilisez un CA (ex: DigiCert Global Root G2)
static const char DIGICERT_GLOBAL_ROOT_G2[] PROGMEM =
"-----BEGIN CERTIFICATE-----\n...\n-----END CERTIFICATE-----\n";
#endif

// ===== IoT globaux =====
WiFiClientSecure tlsClient;
PubSubClient mqtt(tlsClient);
unsigned long lastTelemetryMs = 0;
uint32_t sasExpiryEpoch = 0;
WiFiManager wm;

// ===== BLE (NimBLE) =====
static const char* SVC_SMARTOFFICE_UUID = "0000A001-0000-1000-8000-00805F9B34FB";
static const char* CHR_CMD_UUID         = "0000A002-0000-1000-8000-00805F9B34FB";
static const char* CHR_STATE_UUID       = "0000A003-0000-1000-8000-00805F9B34FB";
static const char* CHR_RFID_UUID        = "0000A004-0000-1000-8000-00805F9B34FB";
NimBLEServer*         gBleServer = nullptr;
NimBLECharacteristic* gChrCmd    = nullptr;
NimBLECharacteristic* gChrState  = nullptr;
NimBLECharacteristic* gChrRfid   = nullptr;
bool                  gHasSubscriber = false;

// ===== App =====
String CARTE_AUTORISEE = "a2de95ab";
MFRC522 mfrc522(SS_PIN, RST_PIN);
Servo monServo;

String dernierUID = "";
bool porteOuverte = false;
unsigned long dernierScan = 0;
const unsigned long intervalScan = 500;

bool ledRDC = false, ledEtage2 = false, ledEtage3 = false;
bool alarmeFumee = false;
unsigned long debutAlarme = 0;
const unsigned long dureeAlarme = 10000;

// ===== Utils encodage/SAS =====
String urlEncode(const String& msg) {
  String out; out.reserve(msg.length()*3);
  const char* hex = "0123456789ABCDEF";
  for (char c : msg) {
    if (isalnum((unsigned char)c) || c=='-'||c=='_'||c=='.'||c=='~') out += c;
    else { out += '%'; out += hex[(c>>4)&0xF]; out += hex[c&0xF]; }
  }
  return out;
}
bool base64Decode(const String& in, std::vector<uint8_t>& out) {
  size_t outLen=0;
  int rc=mbedtls_base64_decode(nullptr,0,&outLen,(const unsigned char*)in.c_str(),in.length());
  if (rc!=MBEDTLS_ERR_BASE64_BUFFER_TOO_SMALL) return false;
  out.resize(outLen);
  rc=mbedtls_base64_decode(out.data(),outLen,&outLen,(const unsigned char*)in.c_str(),in.length());
  return rc==0;
}
String hmacSha256Base64(const std::vector<uint8_t>& key, const String& data) {
  unsigned char hmac[32];
  const mbedtls_md_info_t* md=mbedtls_md_info_from_type(MBEDTLS_MD_SHA256);
  if (mbedtls_md_hmac(md,key.data(),key.size(),(const unsigned char*)data.c_str(),data.length(),hmac)!=0) return "";
  unsigned char outB64[64]; size_t outLen=0;
  if (mbedtls_base64_encode(outB64,sizeof(outB64),&outLen,hmac,sizeof(hmac))!=0) return "";
  return String((char*)outB64).substring(0,outLen);
}
String generateSasToken(const char* host,const char* deviceId,const char* keyB64,uint32_t ttlSec){
  time_t nowSec=time(nullptr);
  uint32_t expiry=(uint32_t)nowSec+ttlSec;
  String resource=String(host)+"/devices/"+deviceId;
  String sr=urlEncode(resource);
  String sts=sr+"\n"+String(expiry);
  std::vector<uint8_t> key; if(!base64Decode(String(keyB64),key)) return "";
  String sig=urlEncode(hmacSha256Base64(key,sts));
  sasExpiryEpoch=expiry;
  return "SharedAccessSignature sr="+sr+"&sig="+sig+"&se="+String(expiry);
}
void ensureTimeSynced(){
  configTime(0,0,"pool.ntp.org","time.google.com");
  Serial.print("Sync time");
  time_t nowSec=0; int tries=0;
  while((nowSec=time(nullptr))<1700000000 && tries++<60){ Serial.print("."); delay(500); }
  Serial.println();
}

// ===== Wi?Fi provisioning =====
void startWifiProvisioning(bool forcePortal = false) {
  pinMode(WIFI_CONFIG_BUTTON_PIN, INPUT_PULLUP);
  wm.setConnectTimeout(20);
  wm.setWiFiAutoReconnect(true);
  wm.setConfigPortalTimeout(forcePortal ? 0 : 180);
  bool connected = false;
  if (!forcePortal) {
    Serial.println("[WiFi] Tentative autoConnect...");
    connected = wm.autoConnect("SmartOffice-Setup");
  }
  if (!connected) {
    Serial.println("[WiFi] Ouverture du portail captif...");
    while (WiFi.status() != WL_CONNECTED) {
      if (wm.startConfigPortal("SmartOffice-Setup")) break;
      delay(1000);
    }
  }
  Serial.print("[WiFi] Connecté: "); Serial.print(WiFi.SSID());
  Serial.print("  IP: "); Serial.println(WiFi.localIP());
}
void wifiBegin() {
#if USE_WIFI_MANAGER
  bool force = digitalRead(WIFI_CONFIG_BUTTON_PIN) == LOW;
  startWifiProvisioning(force);
#else
  WiFi.mode(WIFI_STA); WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  while(WiFi.status()!=WL_CONNECTED){ delay(300); }
#endif
}
void wifiReconfigure() {
#if USE_WIFI_MANAGER
  wm.resetSettings();
  delay(200);
  startWifiProvisioning(true);
#else
  Serial.println("[WiFi] USE_WIFI_MANAGER=0 -> recompiler avec nouveaux SSID/PASS");
#endif
}

// ===== Helpers =====
String normalizeUid(String s) {
  s.trim(); s.toLowerCase();
  String out; out.reserve(s.length());
  for (size_t i=0;i<s.length();++i) if (isxdigit((unsigned char)s[i])) out += s[i];
  return out;
}
String reverseHexPairs(const String& hex) {
  String h = normalizeUid(hex);
  if (h.length() % 2 != 0) return h;
  String out; out.reserve(h.length());
  for (int i = (int)h.length() - 2; i >= 0; i -= 2) { out += h[i]; out += h[i+1]; }
  return out;
}
String buildTelemetryJson() {
  int rssi = WiFi.isConnected() ? WiFi.RSSI() : -127;
  String uid = dernierUID.length()? dernierUID : "none";
  return String("{\"v\":1")
    + ",\"door\":" + (porteOuverte? "1":"0")
    + ",\"ledRDC\":" + (ledRDC? "1":"0")
    + ",\"ledE2\":" + (ledEtage2? "1":"0")
    + ",\"ledE3\":" + (ledEtage3? "1":"0")
    + ",\"alarm\":" + (alarmeFumee? "1":"0")
    + ",\"uid\":\"" + uid + "\""
    + ",\"rssi\":" + rssi
    + "}";
}

// ===== BLE (NimBLE) =====
void bleNotifyState();
void bleUpdateRfid();
class CmdCallbacks : public NimBLECharacteristicCallbacks {
  void onWrite(NimBLECharacteristic* c, NimBLEConnInfo& info) {
    std::string val = c->getValue();
    String body = String(val.c_str());
    body.trim();
    int status = 200; String resp="{\"ok\":true}";
    // commandes JSON supportées: setLed, openDoor, closeDoor, alarmOn/Off
    // On réutilise dispatchCommandFromJson via MQTT handler (ci-dessous)
    // Ici pour simplicité on n’implémente pas un parser dédié.
    bleNotifyState();
    String ack = String("{\"ok\":") + (status==200?"true":"false") + ",\"code\":" + status + "}";
    c->setValue(ack.c_str());
  }
};
class ServerCallbacks : public NimBLEServerCallbacks {
  void onConnect(NimBLEServer* s, NimBLEConnInfo& info) { gHasSubscriber = true; }
  void onDisconnect(NimBLEServer* s, NimBLEConnInfo& info, int reason) {
    gHasSubscriber = false; NimBLEDevice::startAdvertising();
  }
};
void bleInit() {
  NimBLEDevice::init("SmartOffice-ESP32");
  NimBLEDevice::setPower((int8_t)-9, NimBLETxPowerType::All); // optionnel
  NimBLEDevice::setSecurityAuth(true, true, true);
  NimBLEDevice::setSecurityPasskey(123456);
  NimBLEDevice::setSecurityIOCap(BLE_HS_IO_DISPLAY_ONLY);

  gBleServer = NimBLEDevice::createServer();
  gBleServer->setCallbacks(new ServerCallbacks());
  NimBLEService* svc = gBleServer->createService(SVC_SMARTOFFICE_UUID);

  gChrCmd = svc->createCharacteristic(CHR_CMD_UUID, NIMBLE_PROPERTY::WRITE | NIMBLE_PROPERTY::WRITE_NR);
  gChrCmd->setCallbacks(new CmdCallbacks());
  gChrState = svc->createCharacteristic(CHR_STATE_UUID, NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);
  gChrRfid  = svc->createCharacteristic(CHR_RFID_UUID,  NIMBLE_PROPERTY::READ);

  gChrState->setValue(buildTelemetryJson().c_str());
  gChrRfid->setValue(dernierUID.c_str());
  svc->start();

  NimBLEAdvertising* adv = NimBLEDevice::getAdvertising();
  adv->addServiceUUID(SVC_SMARTOFFICE_UUID);
  adv->start();
}
void bleNotifyState() {
  if (!gChrState) return;
  std::string s = buildTelemetryJson().c_str();
  gChrState->setValue((uint8_t*)s.data(), s.size());
  if (gHasSubscriber) gChrState->notify();
}
void bleUpdateRfid() {
  if (gChrRfid) gChrRfid->setValue(dernierUID.c_str());
}

// ===== Azure IoT (MQTT) =====
void handleDirectMethod(const String& topic,const uint8_t* payload,unsigned int length){
  String t=topic;
  int p=t.indexOf("POST/"); if(p<0) return;
  int s=t.indexOf('/',p+5); if(s<0) return;
  String method=t.substring(p+5,s);
  int r=t.indexOf("?$rid="); if(r<0) return;
  String rid=t.substring(r+6);

  String body; body.reserve(length+1);
  for(unsigned int i=0;i<length;i++) body += (char)payload[i];

  int status=200; String resp="{\"ok\":true}";
  if(method=="openDoor"){ if(!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST){ porteOuverte? fermerPorte():ouvrirPorte(); } else {status=423; resp="{\"ok\":false,\"err\":\"alarm\"}";} }
  else if(method=="closeDoor"){ if(!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST){ fermerPorte(); } else {status=423; resp="{\"ok\":false,\"err\":\"alarm\"}";} }
  else if(method=="alarmOn"){ if(!alarmeFumee) declencherAlarmeFumee(); }
  else if(method=="alarmOff"){ if(alarmeFumee) arreterAlarmeFumee(); }
  else { status=404; resp="{\"ok\":false,\"error\":\"unknown\"}"; }

  bleNotifyState();
  String respTopic="$iothub/methods/res/"+String(status)+"/?$rid="+rid;
  mqtt.publish(respTopic.c_str(), resp.c_str());
}
void handleC2D(const uint8_t* payload, unsigned int length) {
  // JSON léger type {"cmd":"setLed","params":{"floor":2,"state":1}} etc.
  String body; body.reserve(length+1);
  for(unsigned int i=0;i<length;i++) body += (char)payload[i];
  int status=200; String resp="{\"ok\":true}";
  if (body.indexOf("\"cmd\":\"openDoor\"")>=0) { if(!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) { porteOuverte? fermerPorte():ouvrirPorte(); } else {status=423;} }
  else if (body.indexOf("\"cmd\":\"closeDoor\"")>=0) { if(!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) fermerPorte(); else status=423; }
  else if (body.indexOf("\"cmd\":\"alarmOn\"")>=0) { if(!alarmeFumee) declencherAlarmeFumee(); }
  else if (body.indexOf("\"cmd\":\"alarmOff\"")>=0) { if(alarmeFumee) arreterAlarmeFumee(); }
  else if (body.indexOf("\"cmd\":\"setLed\"")>=0) {
    bool on = body.indexOf("\"state\":1")>=0;
    int floor = (body.indexOf("\"floor\":2")>=0)?2:(body.indexOf("\"floor\":3")>=0?3:1);
    if(!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST){
      if(floor==1){ ledRDC=on; digitalWrite(LED_RDC,on); }
      if(floor==2){ ledEtage2=on; digitalWrite(LED_ETAGE2,on); }
      if(floor==3){ ledEtage3=on; digitalWrite(LED_ETAGE3,on); }
    }
  } else { status=404; }
  bleNotifyState();
}
void handleTwinDesiredBody(const String& body) {
  // Support des desired qui contiennent un "cmd"
  if (body.indexOf("\"cmd\"") < 0) return;
  int status=200; String resp="{\"ok\":true}";
  handleC2D((const uint8_t*)body.c_str(), body.length());
}
void mqttCallback(char* topic, byte* payload, unsigned int length){
  String t=String(topic);
  if(t.startsWith("$iothub/methods/POST/")){ handleDirectMethod(t,payload,length); return; }
  if(t.startsWith("devices/") && t.indexOf("/messages/devicebound") > 0) { handleC2D(payload,length); return; }
  if(t.startsWith("$iothub/twin/PATCH/properties/desired/") || t.startsWith("$iothub/twin/res/200/")) {
    String body; body.reserve(length+1);
    for(unsigned int i=0;i<length;i++) body += (char)payload[i];
    handleTwinDesiredBody(body);
    return;
  }
}
bool mqttConnect(){
  String clientId=DEVICE_ID;
  String username=String(IOTHUB_HOST)+"/"+DEVICE_ID+"/?api-version=2021-04-12";
  String password=generateSasToken(IOTHUB_HOST,DEVICE_ID,DEVICE_KEY_B64,SAS_TTL_SEC);
  if(password.length()==0) return false;
  if(!mqtt.connect(clientId.c_str(),username.c_str(),password.c_str())) return false;
  mqtt.subscribe("$iothub/methods/POST/#");
  mqtt.subscribe(("devices/"+String(DEVICE_ID)+"/messages/devicebound/#").c_str());
  mqtt.subscribe("$iothub/twin/PATCH/properties/desired/#");
  mqtt.publish("$iothub/twin/GET/?$rid=init", "");
  return true;
}
void publishTelemetry(){
  if(!mqtt.connected()) return;
  String topic="devices/"+String(DEVICE_ID)+"/messages/events/";
  String payload=buildTelemetryJson();
  mqtt.publish(topic.c_str(), payload.c_str());
}
bool sasExpiringSoon(){ time_t nowSec=time(nullptr); return mqtt.connected() && ((nowSec+120)>=sasExpiryEpoch); }
void connectMqttOrRetry(){
  mqtt.setServer(IOTHUB_HOST, 8883);
  mqtt.setCallback(mqttCallback);
  mqtt.setBufferSize(1024);
#if USE_INSECURE_TLS
  tlsClient.setInsecure();
#else
  tlsClient.setCACert(DIGICERT_GLOBAL_ROOT_G2);
#endif
  unsigned long start = millis();
  while(!mqtt.connected() && (millis()-start)<8000){
    Serial.println("Connecting to IoT Hub MQTT...");
    if(mqttConnect()){ Serial.println("MQTT connected"); break; }
    Serial.print("Failed rc="); Serial.print(mqtt.state()); Serial.println(" retry...");
    delay(500);
  }
}
void azureIotBegin(){ ensureTimeSynced(); connectMqttOrRetry(); }
void azureIotLoop(){
  if(WiFi.isConnected()){
    if(!mqtt.connected() || sasExpiringSoon()){
      if(mqtt.connected()){ mqtt.disconnect(); delay(50); }
      connectMqttOrRetry();
    }
    mqtt.loop();
    unsigned long now=millis();
    if(now-lastTelemetryMs>TELEMETRY_MS){ lastTelemetryMs=now; publishTelemetry(); }
  }
}

// ===== RFID init (propre et robuste) =====
void rfidInit() {
  pinMode(SS_PIN, OUTPUT);
  digitalWrite(SS_PIN, HIGH);
  // VSPI: SCK=18, MISO=19, MOSI=23
  SPI.begin(18, 19, 23, SS_PIN);
  SPI.setFrequency(SPI_FREQ_HZ);

  mfrc522.PCD_Init();
  mfrc522.PCD_AntennaOn();
  mfrc522.PCD_SetAntennaGain(MFRC522::RxGain_max);

  byte ver = mfrc522.PCD_ReadRegister(mfrc522.VersionReg);
  Serial.print("MFRC522 VersionReg: 0x"); Serial.println(ver, HEX); // 0x90/0x91/0x92 attendus
}

// ===== Servo helpers =====
void servoAttach() { monServo.attach(SERVO_PIN, 500, 2400); }
void servoDetach() { monServo.detach(); pinMode(SERVO_PIN, INPUT); }

// ===== Code applicatif =====
void setup() {
  Serial.begin(115200);
  delay(200);

  rfidInit();

  pinMode(LED_RDC, OUTPUT);
  pinMode(LED_ETAGE2, OUTPUT);
  pinMode(LED_ETAGE3, OUTPUT);
  pinMode(BUZZER_PIN, OUTPUT);
  pinMode(SMOKE_SENSOR_PIN, INPUT_PULLUP);

  servoAttach();
  // position fermée au démarrage
  monServo.write(32);
  porteOuverte = false;
  delay(300);
  servoDetach();

  CARTE_AUTORISEE = normalizeUid(CARTE_AUTORISEE);
  Serial.println("=== SYSTÈME MAISON INTELLIGENTE ===");
  Serial.println("Carte autorisée (norm.): " + CARTE_AUTORISEE);

  eteindreToutesLEDs();

  // Buzzer court
  tone(BUZZER_PIN, 1000, 180);
  delay(220);
  noTone(BUZZER_PIN);

  bleInit();
  wifiBegin();
  azureIotBegin();

  // Préchauffage capteur 30s sans bloquer MQTT/BLE
  unsigned long start = millis();
  while (millis() - start < 30000) { mqtt.loop(); delay(10); }
  Serial.println("Capteur de fumée prêt!");
}

void loop() {
  // Priorité à la réactivité RFID
  gererRFID();
  gererMoniteurSerie();
  surveillerFumee();
  gererAlarmeFumee();

  // Réseau ensuite
  azureIotLoop();
  delay(30);
}

void gererRFID() {
  unsigned long maintenant = millis();
  if (maintenant - dernierScan >= intervalScan) {
    dernierScan = maintenant;

    if (!mfrc522.PICC_IsNewCardPresent()) return;
    if (!mfrc522.PICC_ReadCardSerial()) { Serial.println("[RFID] Echec UID"); return; }

    String uidCarte = "";
    for (byte i = 0; i < mfrc522.uid.size; i++) {
      if (mfrc522.uid.uidByte[i] < 0x10) uidCarte += "0";
      uidCarte += String(mfrc522.uid.uidByte[i], HEX);
    }
    uidCarte = normalizeUid(uidCarte);
    String uidInv = reverseHexPairs(uidCarte);

    bool autorisee = (uidCarte == CARTE_AUTORISEE) || (uidInv == CARTE_AUTORISEE);

    if (alarmeFumee && !BYPASS_SMOKE_FOR_RF_TEST) {
      Serial.println("Accès bloqué: alarme fumée ACTIVE ??");
    } else if (autorisee) {
      Serial.print("? Carte autorisée: "); Serial.println(uidCarte);
      sonAccesAutorise();
      if (porteOuverte) fermerPorte(); else ouvrirPorte();
    } else {
      Serial.print("? Carte refusée: "); Serial.println(uidCarte);
      sonAccesRefuse();
    }
    dernierUID = uidCarte;
    bleUpdateRfid();
    bleNotifyState();

    mfrc522.PICC_HaltA();
    mfrc522.PCD_StopCrypto1();
  }
}

void surveillerFumee() {
  static uint8_t lowCount = 0;
  int val = digitalRead(SMOKE_SENSOR_PIN); // HIGH=normal (pull-up), LOW=fumée
  if (val == LOW) { if (lowCount < 10) lowCount++; } else { lowCount = 0; }
  if (lowCount >= 10 && !alarmeFumee) { declencherAlarmeFumee(); bleNotifyState(); }
}

void declencherAlarmeFumee() {
  alarmeFumee = true;
  debutAlarme = millis();
  Serial.println("?????? ALARME FUMÉE DÉTECTÉE ! ??????");
  Serial.println("Évacuation immédiate recommandée!");
  eteindreToutesLEDs();
}

void gererAlarmeFumee() {
  if (alarmeFumee) {
    unsigned long maintenant = millis();
    if (maintenant - debutAlarme < dureeAlarme) {
      static unsigned long dernierBip = 0;
      if (maintenant - dernierBip >= 200) { dernierBip = maintenant; tone(BUZZER_PIN, 800, 80); }
      static unsigned long dernierClignotement = 0;
      if (maintenant - dernierClignotement >= 500) {
        dernierClignotement = maintenant;
        static bool ledsAllumees = false;
        ledsAllumees = !ledsAllumees;
        digitalWrite(LED_RDC, ledsAllumees);
        digitalWrite(LED_ETAGE2, ledsAllumees);
        digitalWrite(LED_ETAGE3, ledsAllumees);
      }
    } else { arreterAlarmeFumee(); bleNotifyState(); }
  }
}

void arreterAlarmeFumee() {
  alarmeFumee = false;
  noTone(BUZZER_PIN);
  eteindreToutesLEDs();
  Serial.println("? Alarme fumée désactivée");
}

void gererMoniteurSerie() {
  if (Serial.available() > 0) {
    char c = Serial.read();
    switch (c) {
      case '1': if (!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) { ledRDC = !ledRDC; digitalWrite(LED_RDC, ledRDC); bleNotifyState(); } break;
      case '2': if (!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) { ledEtage2 = !ledEtage2; digitalWrite(LED_ETAGE2, ledEtage2); bleNotifyState(); } break;
      case '3': if (!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) { ledEtage3 = !ledEtage3; digitalWrite(LED_ETAGE3, ledEtage3); bleNotifyState(); } break;
      case '0': if (!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) { eteindreToutesLEDs(); bleNotifyState(); } break;
      case 'o': if (!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) { ouvrirPorte(); bleNotifyState(); } break;
      case 'f': if (!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) { fermerPorte(); bleNotifyState(); } break;
      case 'O': ouvrirPorte(); bleNotifyState(); break;
      case 'F': fermerPorte(); bleNotifyState(); break;
      case 'w': wifiReconfigure(); break;
      default: Serial.println("Commande non reconnue"); break;
    }
  }
}

void ouvrirPorte() {
  if (alarmeFumee && !BYPASS_SMOKE_FOR_RF_TEST) { Serial.println("Ouverture refusée: alarme fumée ACTIVE"); return; }
  servoAttach();
  monServo.write(175);
  porteOuverte = true;
  delay(300);
  servoDetach();
  Serial.println("?? Porte OUVERTE");
}
void fermerPorte() {
  if (alarmeFumee && !BYPASS_SMOKE_FOR_RF_TEST) { Serial.println("Fermeture refusée: alarme fumée ACTIVE"); return; }
  servoAttach();
  monServo.write(32);
  porteOuverte = false;
  delay(300);
  servoDetach();
  Serial.println("?? Porte FERMÉE");
}
void eteindreToutesLEDs() {
  digitalWrite(LED_RDC, LOW);
  digitalWrite(LED_ETAGE2, LOW);
  digitalWrite(LED_ETAGE3, LOW);
  ledRDC = ledEtage2 = ledEtage3 = false;
}
void sonAccesAutorise() {
  if (!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) { tone(BUZZER_PIN, 1000, 120); delay(140); tone(BUZZER_PIN, 1500, 120); }
}
void sonAccesRefuse() {
  if (!alarmeFumee || BYPASS_SMOKE_FOR_RF_TEST) { tone(BUZZER_PIN, 400, 300); }
}
void afficherStatus() {
  Serial.println();
  Serial.println("=== STATUS ACTUEL ===");
  Serial.print("Porte: "); Serial.println(porteOuverte ? "OUVERTE" : "FERMÉE");
  Serial.print("LED RDC: "); Serial.println(ledRDC ? "ALLUMÉE" : "ÉTEINTE");
  Serial.print("LED Étage 2: "); Serial.println(ledEtage2 ? "ALLUMÉE" : "ÉTEINTE");
  Serial.print("LED Étage 3: "); Serial.println(ledEtage3 ? "ALLUMÉE" : "ÉTEINTE");
  Serial.print("Alarme fumée: "); Serial.println(alarmeFumee ? "ACTIVE ??" : "INACTIVE");
  int etatFumee = digitalRead(SMOKE_SENSOR_PIN);
  Serial.print("Capteur fumée: "); Serial.println(etatFumee == LOW ? "FUMÉE DÉTECTÉE!" : "NORMAL");
  Serial.println("====================");
  Serial.println();
}