namespace RydePlannr.Application.DTOs.Message;

public class MessageResponseDto
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Username { get; set; } =  string.Empty;
    public DateTime SentAt { get; set; }
}