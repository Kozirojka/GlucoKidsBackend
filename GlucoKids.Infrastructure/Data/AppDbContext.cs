using GlucoKids.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GlucoKids.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();
    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();
    public DbSet<FoodEntry> FoodEntries => Set<FoodEntry>();
    public DbSet<MedicalProfile> MedicalProfiles => Set<MedicalProfile>();
    public DbSet<XpLog> XpLogs => Set<XpLog>();
    public DbSet<GlucoseReading> GlucoseReadings => Set<GlucoseReading>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<ChildAchievement> ChildAchievements => Set<ChildAchievement>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.FirebaseUid).HasMaxLength(128).IsRequired();
            e.HasIndex(x => x.FirebaseUid).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.DisplayName).HasMaxLength(100);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
        });

        model.Entity<Child>(e =>
        {
            e.ToTable("children");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasOne(x => x.User)
             .WithOne(u => u.Child)
             .HasForeignKey<Child>(x => x.Id)
             .OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<LessonProgress>(e =>
        {
            e.ToTable("lesson_progress");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ChildId, x.LessonKey }).IsUnique();
            e.Property(x => x.LessonKey).HasMaxLength(128).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Child).WithMany(c => c.Progress).HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<HealthRecord>(e =>
        {
            e.ToTable("health_records");
            e.HasKey(x => x.Id);
            e.Property(x => x.GlucoseMmol).HasColumnType("numeric(5,2)");
            e.Property(x => x.InsulinLong).HasColumnType("numeric(6,2)");
            e.Property(x => x.InsulinShort).HasColumnType("numeric(6,2)");
            e.Property(x => x.CarbohydratesG).HasColumnType("numeric(7,2)");
            e.Property(x => x.MealContext).HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.Child).WithMany().HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<FoodEntry>(e =>
        {
            e.ToTable("food_entries");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Brand).HasMaxLength(100);
            e.Property(x => x.CarbohydratesG).HasColumnType("numeric(7,2)");
            e.Property(x => x.BreadUnits).HasColumnType("numeric(7,2)");
            e.Property(x => x.WeightG).HasColumnType("numeric(7,2)");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.HealthRecord).WithMany(r => r.Foods).HasForeignKey(x => x.HealthRecordId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<MedicalProfile>(e =>
        {
            e.ToTable("medical_profiles");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChildId).IsUnique();
            e.Property(x => x.DiabetesType).HasConversion<string>().HasMaxLength(10).IsRequired();
            e.Property(x => x.TargetGlucoseMin).HasColumnType("numeric(5,2)");
            e.Property(x => x.TargetGlucoseMax).HasColumnType("numeric(5,2)");
            e.Property(x => x.InsulinBrand).HasMaxLength(100);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.Child).WithOne(c => c.MedicalProfile).HasForeignKey<MedicalProfile>(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<XpLog>(e =>
        {
            e.ToTable("xp_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason).HasConversion<string>().HasMaxLength(30).IsRequired();
            e.Property(x => x.ReferenceId).HasMaxLength(128);
            e.Property(x => x.EarnedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.Child).WithMany(c => c.XpLogs).HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<GlucoseReading>(e =>
        {
            e.ToTable("glucose_readings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Value).HasColumnType("numeric(5,2)");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.Child).WithMany().HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<Achievement>(e =>
        {
            e.ToTable("achievements");
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Key).IsUnique();
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
            e.Property(x => x.IconEmoji).HasMaxLength(10).IsRequired();
            e.Property(x => x.Category).HasConversion<string>().HasMaxLength(20).IsRequired();
        });

        model.Entity<ChildAchievement>(e =>
        {
            e.ToTable("child_achievements");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ChildId, x.AchievementId }).IsUnique();
            e.Property(x => x.EarnedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.Child)
             .WithMany()
             .HasForeignKey(x => x.ChildId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Achievement)
             .WithMany(a => a.ChildAchievements)
             .HasForeignKey(x => x.AchievementId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(model);
    }

    private static void SeedData(ModelBuilder model)
    {
        model.Entity<Achievement>().HasData(
            new Achievement { Id = 1,  Key = "first_lesson", Title = "Перший урок",   Description = "Заверши свій перший урок",             IconEmoji = "📖", XpReward = 10,  Category = AchievementCategory.Lessons },
            new Achievement { Id = 2,  Key = "lessons_5",    Title = "5 уроків",       Description = "Заверши 5 уроків",                     IconEmoji = "🎓", XpReward = 50,  Category = AchievementCategory.Lessons },
            new Achievement { Id = 3,  Key = "all_lessons",  Title = "Всі уроки",      Description = "Заверши всі уроки програми",           IconEmoji = "🏆", XpReward = 200, Category = AchievementCategory.Lessons },
            new Achievement { Id = 4,  Key = "first_record", Title = "Перший запис",   Description = "Збережи свій перший запис глюкози",    IconEmoji = "🩸", XpReward = 10,  Category = AchievementCategory.Glucose },
            new Achievement { Id = 5,  Key = "records_7",    Title = "7 записів",      Description = "Збережи 7 записів глюкози",            IconEmoji = "📊", XpReward = 30,  Category = AchievementCategory.Glucose },
            new Achievement { Id = 6,  Key = "records_30",   Title = "30 записів",     Description = "Збережи 30 записів глюкози",           IconEmoji = "📈", XpReward = 100, Category = AchievementCategory.Glucose },
            new Achievement { Id = 7,  Key = "in_range_5",   Title = "У нормі",        Description = "5 показників у цільовому діапазоні",   IconEmoji = "✅", XpReward = 50,  Category = AchievementCategory.Glucose },
            new Achievement { Id = 8,  Key = "first_win",    Title = "Перша перемога", Description = "Виграй свій перший батл",              IconEmoji = "⚔️", XpReward = 20,  Category = AchievementCategory.Battle  },
            new Achievement { Id = 9,  Key = "wins_5",       Title = "5 перемог",      Description = "Виграй 5 батлів",                      IconEmoji = "🥇", XpReward = 75,  Category = AchievementCategory.Battle  },
            new Achievement { Id = 10, Key = "wins_10",      Title = "10 перемог",     Description = "Виграй 10 батлів",                     IconEmoji = "👑", XpReward = 150, Category = AchievementCategory.Battle  },
            new Achievement { Id = 11, Key = "streak_3",     Title = "3 дні поспіль",  Description = "Роби записи 3 дні підряд",             IconEmoji = "🔥", XpReward = 20,  Category = AchievementCategory.Streak  },
            new Achievement { Id = 12, Key = "streak_7",     Title = "Тижневий стрік", Description = "Роби записи 7 днів підряд",            IconEmoji = "⚡", XpReward = 50,  Category = AchievementCategory.Streak  },
            new Achievement { Id = 13, Key = "streak_30",    Title = "Місячний стрік", Description = "Роби записи 30 днів підряд",           IconEmoji = "💎", XpReward = 200, Category = AchievementCategory.Streak  }
        );
    }
}
