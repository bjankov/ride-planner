namespace RydePlannr.Application.DTOs.User;

public class PublicUserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}