using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entité Zone vers DTO de réponse
            CreateMap<Zone, ZoneDto>();

            // DTO de création de zone vers entité Zone
            CreateMap<ZoneCreationDto, Zone>();

            // DTO de mise à jour de zone vers entité Zone (pour la mise à jour)
            CreateMap<ZoneUpdateDto, Zone>();

            // Entité PeripheriqueLogique vers DTO de réponse
            CreateMap<PeripheriqueLogique, PeripheriqueLogiqueDto>();

            // DTO de création vers entité PeripheriqueLogique
            CreateMap<PeripheriqueLogiqueCreationDto, PeripheriqueLogique>();

            // DTO de mise à jour vers entité PeripheriqueLogique
            CreateMap<PeripheriqueLogiqueUpdateDto, PeripheriqueLogique>();

            // Entité Telemetrie vers DTO de réponse
            CreateMap<Telemetrie, TelemetrieDto>()
                .ForMember(dest => dest.Valeur, opt => opt.MapFrom(src => (decimal)src.Valeur));

            // DTO de création de Telemetrie vers entité Telemetrie
            CreateMap<TelemetrieDto, Telemetrie>()
                .ForMember(dest => dest.Valeur, opt => opt.MapFrom(src => (double)src.Valeur));
        }
    }
}
