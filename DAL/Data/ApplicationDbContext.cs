using Microsoft.EntityFrameworkCore;
using Rovaya.DAL.Models.TourismGuide;

namespace Rovaya.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TourismGuideCategory> TourismGuideCategories { get; set; }
        public DbSet<TourismGuideCategoryTranslation> TourismGuideCategoryTranslations { get; set; }
        public DbSet<TourismGuidePlace> TourismGuidePlaces { get; set; }
        public DbSet<TourismGuidePlaceTranslation> TourismGuidePlaceTranslations { get; set; }
        public DbSet<CitySection> CitySections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
}