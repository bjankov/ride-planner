namespace RydePlannr.Domain.Entities;

public class ClubMember
{
    public int Id { get; set; }
    public DateTime JoinedAt { get; set; }

    public int UserId { get; set; }
    public int ClubId { get; set; }
    public User User { get; set; } = null!;
    public Club Club { get; set; } = null!;
}