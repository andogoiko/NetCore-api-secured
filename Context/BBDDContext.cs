using apiSecurizada.Models;
using apiSecurizada.Models.DTO;
using Microsoft.EntityFrameworkCore;

public class BBDDContext : DbContext
{
    public BBDDContext(DbContextOptions<BBDDContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<User> Users { get; set; }

     protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

                // Realción 1:1 entre RefreshToken y User
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithOne(u => u.RefreshToken)
                .HasForeignKey<RefreshToken>(rt => rt.UserId);


        }

}