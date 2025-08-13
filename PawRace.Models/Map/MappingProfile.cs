using AutoMapper;
using PawRace.DataAccess.Models;
using System.Xml.Serialization;
using Db = PawRace.DataAccess.Models;
using Dto = PawRace.Models.DTO;

namespace PawRace.Models.Map
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Db.Dog, Dto.Dog>();
            CreateMap<Dto.Race, Db.Race>()
                    .ForMember(dest=> dest.RaceStatusId, opt=> opt.MapFrom(src=> src.RaceStatus));
            CreateMap< Db.Race, Dto.Race>()
                .ForMember(dest => dest.RaceStatus, opt => opt.MapFrom(src => src.RaceStatusId));
            CreateMap<Db.Race, Dto.RaceDetails>()
                .ForMember(dest=> dest.Id, opt=> opt.MapFrom(src=> src.Id))
                .ForMember(dest=> dest.RacingDogs, opt=> opt.MapFrom(src => src.RaceDogs))
                .ForMember(dest=> dest.ScheduledAt, opt=> opt.MapFrom(src=> src.ScheduledAt));
            CreateMap<Dto.RacingDog, Db.RaceDog>().ReverseMap();
            CreateMap<Dto.User, Db.User>().ReverseMap();
            CreateMap<Dto.BetTicket, Db.Ticket>().ReverseMap();
            CreateMap<Db.Ticket, Dto.TicketStatus>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusId))
                .ForMember(dest => dest.NickName, opt => opt.MapFrom(src => src.User.NickName))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.User.IpAddress));

        }
    }
}
