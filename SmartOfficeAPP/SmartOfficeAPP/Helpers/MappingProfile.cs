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
        }
    }
}
