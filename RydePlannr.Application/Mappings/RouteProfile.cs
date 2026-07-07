using AutoMapper;
using RydePlannr.Application.DTOs.Location;
using RydePlannr.Application.DTOs.Route;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Application.Mappings;

public class RouteProfile :  Profile
{
    public RouteProfile()
    {
        CreateMap<Location, LocationResponseDto>();
        CreateMap<CreateLocationDto, Location>();

        CreateMap<CreateRouteDto, Route>();
        CreateMap<Route, RouteResponseDto>()
            .ForMember(dest => dest.StartLocationName, opt => opt.MapFrom(src => src.StartLocation.Name))
            .ForMember(dest => dest.EndLocationName, opt => opt.MapFrom(src => src.EndLocation.Name))
            .ForMember(dest => dest.Surface, opt => opt.MapFrom(src => src.Surface.ToString()))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Difficulty.ToString()));
    }
}