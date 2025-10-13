using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Zone ↔ DTOs
            CreateMap<Zone, ZoneDto>()
                .ForMember(dest => dest.Nom, opt => opt.MapFrom(src => src.Name));

            CreateMap<ZoneCreationDto, Zone>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Nom));

            CreateMap<ZoneUpdateDto, Zone>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Nom));

            // Périphériques
            CreateMap<PeripheriqueLogique, PeripheriqueLogiqueDto>();
            CreateMap<PeripheriqueLogiqueCreationDto, PeripheriqueLogique>();
            CreateMap<PeripheriqueLogiqueUpdateDto, PeripheriqueLogique>();

            // Télémétrie
            CreateMap<Telemetrie, TelemetrieDto>()
                .ForMember(dest => dest.Valeur, opt => opt.MapFrom(src => (decimal)src.Valeur));
            CreateMap<TelemetrieDto, Telemetrie>()
                .ForMember(dest => dest.Valeur, opt => opt.MapFrom(src => (double)src.Valeur));

            // IncidentTicket ↔ DTOs
            CreateMap<IncidentTicket, IncidentTicketDto>();
            CreateMap<IncidentTicketCreateDto, IncidentTicket>()
                .ForMember(dest => dest.DateCreation, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DateResolution, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Zone, opt => opt.Ignore())
                .ForMember(dest => dest.PeripheriqueLogique, opt => opt.Ignore());
        }
    }
}
