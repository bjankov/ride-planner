namespace RydePlannr.Application.DTOs.Club;

public class ClubMemberResponseDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}