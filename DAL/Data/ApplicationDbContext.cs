using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.Identity;
using Rovaya.DAL.Models.TourismGuide;

namespace Rovaya.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<TourismGuideCategory> TourismGuideCategories { get; set; }
        public DbSet<TourismGuideCategoryTranslation> TourismGuideCategoryTranslations { get; set; }
        public DbSet<TourismGuidePlace> TourismGuidePlaces { get; set; }
        public DbSet<TourismGuidePlaceTranslation> TourismGuidePlaceTranslations { get; set; }






        public DbSet<CitySection> CitySections { get; set; }
        public DbSet<UserMedicalProfile> UserMedicalProfiles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");



            modelBuilder.Entity<TourismGuideCategoryTranslation>()
                .HasOne(t => t.TourismGuideCategory)
                .WithMany(c => c.TourismGuideTranslations)
                .HasForeignKey(t => t.TourismGuideCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TourismGuidePlaceTranslation>()
                .HasOne(t => t.TourismGuidePlace)
                .WithMany(p => p.Translations)
                .HasForeignKey(t => t.TourismGuidePlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TourismGuidePlace>()
                .HasOne(p => p.ParentPlace)
                .WithMany(p => p.SubPlaces)
                .HasForeignKey(p => p.ParentPlaceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CitySection>()
                .HasOne(s => s.City)
                .WithMany()
                .HasForeignKey(s => s.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserMedicalProfile>()
    .HasOne(m => m.ApplicationUser)
    .WithOne(u => u.MedicalProfile)
    .HasForeignKey<UserMedicalProfile>(m => m.ApplicationUserId)
    .OnDelete(DeleteBehavior.Cascade);








        }
    }
}