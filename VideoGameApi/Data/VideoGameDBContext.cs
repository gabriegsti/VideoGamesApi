using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VideoGameApi.Entities;

namespace VideoGameApi.Data
{
    public class VideoGameDBContext : IdentityDbContext<IdentityUser>
    {
        public VideoGameDBContext(DbContextOptions<VideoGameDBContext> options) : base(options)
        {
        }

        public DbSet<VideoGame> VideoGames { get; set; }

        public DbSet<UserGame> UserGames { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserGame>()
                .HasKey(x => new { x.UserId, x.VideoGameId });

            builder.Entity<UserGame>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserGame>()
                .HasOne(x => x.VideoGame)
                .WithMany()
                .HasForeignKey(x => x.VideoGameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
