namespace VideoGameApi.Entities
{
    public class UserGame
    {
        public string UserId { get; set; } = string.Empty;

        public int VideoGameId { get; set; }

        public bool IsOwned { get; set; }

        public bool IsPlayed { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public VideoGame VideoGame { get; set; } = null!;
    }
}
