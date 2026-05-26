using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportConnect.Domain.Entities;

namespace SportConnect.Data.Configurations;

public class ActivityParticipantConfiguration : IEntityTypeConfiguration<ActivityParticipant>
{
    public void Configure(EntityTypeBuilder<ActivityParticipant> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.Activity)
               .WithMany(a => a.Participants)
               .HasForeignKey(p => p.ActivityId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
               .WithMany(u => u.Participations)
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.ActivityId, p.UserId }).IsUnique();
    }
}

public class UserLanguageConfiguration : IEntityTypeConfiguration<UserLanguage>
{
    public void Configure(EntityTypeBuilder<UserLanguage> builder)
    {
        builder.HasKey(ul => new { ul.UserId, ul.LanguageId });

        builder.HasOne(ul => ul.User)
               .WithMany(u => u.Languages)
               .HasForeignKey(ul => ul.UserId);

        builder.HasOne(ul => ul.Language)
               .WithMany(l => l.Users)
               .HasForeignKey(ul => ul.LanguageId);
    }
}

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.HasKey(ua => new { ua.UserId, ua.AchievementId });

        builder.HasOne(ua => ua.User)
               .WithMany(u => u.Achievements)
               .HasForeignKey(ua => ua.UserId);

        builder.HasOne(ua => ua.Achievement)
               .WithMany(a => a.UnlockedBy)
               .HasForeignKey(ua => ua.AchievementId);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Content).IsRequired().HasMaxLength(2000);

        builder.HasOne(m => m.Activity)
               .WithMany(a => a.Messages)
               .HasForeignKey(m => m.ActivityId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
               .WithMany(u => u.Messages)
               .HasForeignKey(m => m.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.ActivityId, m.SentAt });
    }
}

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(500);
        builder.Property(a => a.IconName).HasMaxLength(50);

        builder.HasIndex(a => a.Code).IsUnique();
    }
}

public class SportConfiguration : IEntityTypeConfiguration<Sport>
{
    public void Configure(EntityTypeBuilder<Sport> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(60);
        builder.HasIndex(s => s.Name).IsUnique();
    }
}

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Code).IsRequired().HasMaxLength(5);
        builder.HasIndex(l => l.Code).IsUnique();
    }
}
