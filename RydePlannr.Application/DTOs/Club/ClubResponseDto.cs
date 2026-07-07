namespace RydePlannr.Application.DTOs.Club;

public class ClubResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime FoundedAt { get; set; }
    public int MemberCount { get; set; }
}