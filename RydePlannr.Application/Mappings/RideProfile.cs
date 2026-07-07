using AutoMapper;
using RydePlannr.Application.DTOs.Message;
using RydePlannr.Application.DTOs.RideEvent;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Application.Mappings;

public class RideProfile : Profile
{
    public RideProfile()
    {
        CreateMap<CreateRideEventDto, RideEvent>();

        CreateMap<RideEvent, RideEventResponseDto>()
            .ForMember(dest => dest.OrganizerUsername,
                opt => opt.MapFrom(src => src.Organizer.Username))
            .ForMember(dest => dest.RouteName,
                opt => opt.MapFrom(src => src.Route.Name))
            .ForMember(dest => dest.RideTypeName,
                opt => opt.MapFrom(src => src.RideType.Name))
            .ForMember(dest => dest.ClubName,
                opt => opt.MapFrom(src => src.Club != null ? src.Club.Name : null))
            .ForMember(dest => dest.CurrentParticipants,
                opt => opt.MapFrom(src => src.Participants.Count))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CutoffAt,
                opt => opt.MapFrom(src => src.CutoffMinutes.HasValue
                    ? src.StartTime.AddMinutes(src.CutoffMinutes.Value)
                    : (DateTime?)null));
        
        CreateMap<RideMessage, MessageResponseDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));

        CreateMap<CreateMessageDto, RideMessage>();
    }
}