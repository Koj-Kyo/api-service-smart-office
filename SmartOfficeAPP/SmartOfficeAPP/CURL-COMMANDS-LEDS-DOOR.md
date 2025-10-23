# ?? Commandes cURL: Contrôle LEDs et Porte

## ?? Guide Ligne de Commande

Ce fichier contient toutes les commandes **cURL** prêtes à l'emploi pour contrôler les LEDs et la porte via terminal/PowerShell.

---

## ?? Configuration Initiale

### **Variables d'environnement:**
```bash
# Pour Linux/Mac (bash/zsh)
export BASE_URL="https://localhost:7169"
export DEVICE_ID="mon-esp32"

# Pour Windows (PowerShell)
$BASE_URL = "https://localhost:7169"
$DEVICE_ID = "mon-esp32"
```

---

## ?? 1. Authentification

### **Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@smartoffice.com",
    "password": "Admin@123"
  }' \
  -k | jq -r '.token'
```

### **Windows PowerShell:**
```powershell
$response = Invoke-RestMethod -Uri "$BASE_URL/api/Auth/login" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"email":"admin@smartoffice.com","password":"Admin@123"}' `
  -SkipCertificateCheck

$TOKEN = $response.token
Write-Host "Token: $TOKEN"
```

### **Enregistrer le token:**
```bash
# Linux/Mac
export TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Windows PowerShell
$TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## ?? 2. Contrôle LED Étage 2

### **Allumer LED Étage 2:**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage2/commands/on" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k
```

**Windows PowerShell:**
```powershell
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage2/commands/on" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck
```

---

### **Éteindre LED Étage 2:**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage2/commands/off" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k
```

**Windows PowerShell:**
```powershell
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage2/commands/off" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck
```

---

### **Basculer LED Étage 2 (Toggle):**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage2/commands/toggle" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k
```

**Windows PowerShell:**
```powershell
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage2/commands/toggle" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck
```

---

## ?? 3. Contrôle LED Étage 3

### **Allumer LED Étage 3:**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage3/commands/on" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k
```

**Windows PowerShell:**
```powershell
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage3/commands/on" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck
```

---

### **Éteindre LED Étage 3:**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage3/commands/off" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k
```

**Windows PowerShell:**
```powershell
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage3/commands/off" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck
```

---

### **Basculer LED Étage 3 (Toggle):**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage3/commands/toggle" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k
```

**Windows PowerShell:**
```powershell
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage3/commands/toggle" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck
```

---

## ?? 4. Contrôle LED RDC

### **Allumer LED RDC:**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_rdc/commands/on" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k
```

**Windows PowerShell:**
```powershell
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_rdc/commands/on" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck
```

---

## ?? 5. Contrôle de la Porte

### **Ouvrir la porte:**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/IoTHub/device/${DEVICE_ID}/method" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"methodName":"openDoor","payload":{}}' \
  -k
```

**Windows PowerShell:**
```powershell
$body = @{
    methodName = "openDoor"
    payload = @{}
} | ConvertTo-Json

Invoke-RestMethod -Uri "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -ContentType "application/json" `
  -Body $body `
  -SkipCertificateCheck
```

---

### **Fermer la porte:**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/IoTHub/device/${DEVICE_ID}/method" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"methodName":"closeDoor","payload":{}}' \
  -k
```

**Windows PowerShell:**
```powershell
$body = @{
    methodName = "closeDoor"
    payload = @{}
} | ConvertTo-Json

Invoke-RestMethod -Uri "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -ContentType "application/json" `
  -Body $body `
  -SkipCertificateCheck
```

---

## ?? 6. Vérifications

### **Voir l'état d'une LED:**

**Linux/Mac:**
```bash
curl -X GET "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage2" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k | jq
```

**Windows PowerShell:**
```powershell
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage2" `
  -Method GET `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck | ConvertTo-Json
```

---

### **Voir l'état du device complet:**

**Linux/Mac:**
```bash
curl -X POST "${BASE_URL}/api/IoTHub/device/${DEVICE_ID}/method" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"methodName":"getStatus","payload":{}}' \
  -k | jq
```

**Windows PowerShell:**
```powershell
$body = @{
    methodName = "getStatus"
    payload = @{}
} | ConvertTo-Json

Invoke-RestMethod -Uri "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -ContentType "application/json" `
  -Body $body `
  -SkipCertificateCheck | ConvertTo-Json
```

---

### **Voir tous les périphériques:**

**Linux/Mac:**
```bash
curl -X GET "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k | jq
```

**Windows PowerShell:**
```powershell
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals" `
  -Method GET `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck | ConvertTo-Json
```

---

## ?? 7. Scripts de Scénarios

### **Scénario 1: Allumer Étage 2 + Ouvrir Porte**

**Linux/Mac:**
```bash
#!/bin/bash
# Sauvegarder dans: allumer_etage2_ouvrir.sh

# Allumer LED Étage 2
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage2/commands/on" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k

echo "? LED Étage 2 allumée"

# Attendre 1 seconde
sleep 1

# Ouvrir la porte
curl -X POST "${BASE_URL}/api/IoTHub/device/${DEVICE_ID}/method" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"methodName":"openDoor","payload":{}}' \
  -k

echo "? Porte ouverte"
```

**Windows PowerShell:**
```powershell
# Sauvegarder dans: allumer_etage2_ouvrir.ps1

# Allumer LED Étage 2
Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage2/commands/on" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -SkipCertificateCheck

Write-Host "? LED Étage 2 allumée"

# Attendre 1 seconde
Start-Sleep -Seconds 1

# Ouvrir la porte
$body = @{methodName="openDoor";payload=@{}} | ConvertTo-Json
Invoke-RestMethod -Uri "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -ContentType "application/json" `
  -Body $body `
  -SkipCertificateCheck

Write-Host "? Porte ouverte"
```

---

### **Scénario 2: Éteindre Toutes LEDs + Fermer Porte**

**Linux/Mac:**
```bash
#!/bin/bash
# Sauvegarder dans: eteindre_tout.sh

# Éteindre LED RDC
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_rdc/commands/off" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k

# Éteindre LED Étage 2
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage2/commands/off" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k

# Éteindre LED Étage 3
curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/led_etage3/commands/off" \
  -H "Authorization: Bearer ${TOKEN}" \
  -k

echo "? Toutes les LEDs éteintes"

sleep 1

# Fermer la porte
curl -X POST "${BASE_URL}/api/IoTHub/device/${DEVICE_ID}/method" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"methodName":"closeDoor","payload":{}}' \
  -k

echo "? Porte fermée"
echo "?? Bonne nuit!"
```

**Windows PowerShell:**
```powershell
# Sauvegarder dans: eteindre_tout.ps1

# Éteindre toutes les LEDs
@("led_rdc", "led_etage2", "led_etage3") | ForEach-Object {
    Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/$_/commands/off" `
      -Method POST `
      -Headers @{"Authorization"="Bearer $TOKEN"} `
      -SkipCertificateCheck
}

Write-Host "? Toutes les LEDs éteintes"

Start-Sleep -Seconds 1

# Fermer la porte
$body = @{methodName="closeDoor";payload=@{}} | ConvertTo-Json
Invoke-RestMethod -Uri "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" `
  -Method POST `
  -Headers @{"Authorization"="Bearer $TOKEN"} `
  -ContentType "application/json" `
  -Body $body `
  -SkipCertificateCheck

Write-Host "? Porte fermée"
Write-Host "?? Bonne nuit!"
```

---

## ??? 8. Fonctions Utilitaires

### **Fonction: Contrôler une LED**

**Linux/Mac (bash):**
```bash
# Ajouter dans ~/.bashrc ou ~/.zshrc

function control_led() {
  local led_name=$1
  local action=$2  # on, off, toggle
  
  curl -X POST "${BASE_URL}/api/Devices/${DEVICE_ID}/peripherals/${led_name}/commands/${action}" \
    -H "Authorization: Bearer ${TOKEN}" \
    -k
  
  echo "? LED ${led_name} ? ${action}"
}

# Usage:
# control_led led_etage2 on
# control_led led_etage3 off
# control_led led_rdc toggle
```

**Windows PowerShell:**
```powershell
# Ajouter dans $PROFILE

function Control-LED {
    param(
        [string]$LedName,
        [string]$Action  # on, off, toggle
    )
    
    Invoke-RestMethod -Uri "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/$LedName/commands/$Action" `
      -Method POST `
      -Headers @{"Authorization"="Bearer $TOKEN"} `
      -SkipCertificateCheck
    
    Write-Host "? LED $LedName ? $Action"
}

# Usage:
# Control-LED -LedName "led_etage2" -Action "on"
# Control-LED -LedName "led_etage3" -Action "off"
```

---

### **Fonction: Contrôler la porte**

**Linux/Mac (bash):**
```bash
function control_door() {
  local action=$1  # open, close
  
  local method="openDoor"
  if [ "$action" == "close" ]; then
    method="closeDoor"
  fi
  
  curl -X POST "${BASE_URL}/api/IoTHub/device/${DEVICE_ID}/method" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json" \
    -d "{\"methodName\":\"${method}\",\"payload\":{}}" \
    -k
  
  echo "? Porte ${action}"
}

# Usage:
# control_door open
# control_door close
```

**Windows PowerShell:**
```powershell
function Control-Door {
    param(
        [ValidateSet("open", "close")]
        [string]$Action
    )
    
    $method = if ($Action -eq "open") { "openDoor" } else { "closeDoor" }
    
    $body = @{
        methodName = $method
        payload = @{}
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" `
      -Method POST `
      -Headers @{"Authorization"="Bearer $TOKEN"} `
      -ContentType "application/json" `
      -Body $body `
      -SkipCertificateCheck
    
    Write-Host "? Porte $Action"
}

# Usage:
# Control-Door -Action "open"
# Control-Door -Action "close"
```

---

## ?? Quick Commands (One-liners)

### **Linux/Mac:**
```bash
# Allumer Étage 2
curl -X POST "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage2/commands/on" -H "Authorization: Bearer $TOKEN" -k

# Allumer Étage 3
curl -X POST "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage3/commands/on" -H "Authorization: Bearer $TOKEN" -k

# Ouvrir porte
curl -X POST "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"methodName":"openDoor","payload":{}}' -k

# Fermer porte
curl -X POST "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"methodName":"closeDoor","payload":{}}' -k
```

### **Windows PowerShell:**
```powershell
# Allumer Étage 2
iwr "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage2/commands/on" -Method POST -Headers @{"Authorization"="Bearer $TOKEN"} -SkipCertificateCheck

# Allumer Étage 3
iwr "$BASE_URL/api/Devices/$DEVICE_ID/peripherals/led_etage3/commands/on" -Method POST -Headers @{"Authorization"="Bearer $TOKEN"} -SkipCertificateCheck

# Ouvrir porte
iwr "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" -Method POST -Headers @{"Authorization"="Bearer $TOKEN"} -ContentType "application/json" -Body '{"methodName":"openDoor","payload":{}}' -SkipCertificateCheck

# Fermer porte
iwr "$BASE_URL/api/IoTHub/device/$DEVICE_ID/method" -Method POST -Headers @{"Authorization"="Bearer $TOKEN"} -ContentType "application/json" -Body '{"methodName":"closeDoor","payload":{}}' -SkipCertificateCheck
```

---

## ?? Ressources Complémentaires

- **Guide visuel:** `QUICK-START-LEDS-DOOR.md`
- **Tests HTTP:** `Tests/16-LEDs-Door-Control.http`
- **Documentation complète:** `FIRMWARE-BACKEND-README.md`

---

**Version:** 1.0.0  
**Date:** 20 Janvier 2025  
**Compatibilité:** Firmware v1.3.0 + Backend .NET 9
