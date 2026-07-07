using AutoMapper;
using RydePlannr.Application.DTOs.Club;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Application.Mappings;

public class ClubProfile :  Profile
{
    public ClubProfile() {
        CreateMap<CreateClubDto, Club>();
        CreateMap<Club, ClubResponseDto>()
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count))
            .ForMember(dest => dest.FoundedAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<ClubMember, ClubMemberResponseDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));

    }
}