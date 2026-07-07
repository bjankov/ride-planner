using AutoMapper;
using RydePlannr.Application.DTOs.User;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Application.Mappings;

public class UserProfile :  Profile
{
    public UserProfile()
    {
        // User → UserResponseDto
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name));

        // User → PublicUserResponseDto
        CreateMap<User, PublicUserResponseDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name));
    }
}