using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportConnect.Domain.Entities;

namespace SportConnect.Data.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Location).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(2000);

        builder.HasOne(a => a.Creator)
               .WithMany(u => u.CreatedActivities)
               .HasForeignKey(a => a.CreatorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Sport)
               .WithMany(s => s.Activities)
               .HasForeignKey(a => a.SportId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.ScheduledAt);
    }
}
