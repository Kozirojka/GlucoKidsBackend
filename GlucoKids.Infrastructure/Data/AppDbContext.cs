using GlucoKids.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GlucoKids.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<ParentChildLink> ParentChildLinks => Set<ParentChildLink>();
    public DbSet<PsychoModule> PsychoModules => Set<PsychoModule>();
    public DbSet<PsychoLesson> PsychoLessons => Set<PsychoLesson>();
    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();
    public DbSet<MatchmakingQueue> MatchmakingQueue => Set<MatchmakingQueue>();
    public DbSet<Duel> Duels => Set<Duel>();
    public DbSet<GlucoseReading> GlucoseReadings => Set<GlucoseReading>();
    public DbSet<DuelPointEvent> DuelPointEvents => Set<DuelPointEvent>();
    public DbSet<ChildStats> ChildStats => Set<ChildStats>();
    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();

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

        model.Entity<Parent>(e =>
        {
            e.ToTable("parents");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.HasOne(x => x.User)
             .WithOne(u => u.Parent)
             .HasForeignKey<Parent>(x => x.Id)
             .OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<ParentChildLink>(e =>
        {
            e.ToTable("parent_child_links");
            e.HasKey(x => x.Id);
            e.Property(x => x.InviteCode).HasMaxLength(8).IsRequired();
            e.HasIndex(x => x.InviteCode).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.ParentUser).WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ChildUser).WithMany().HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Restrict);
        });

        model.Entity<PsychoModule>(e =>
        {
            e.ToTable("psycho_modules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
        });

        model.Entity<PsychoLesson>(e =>
        {
            e.ToTable("psycho_lessons");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.HasOne(x => x.Module)
             .WithMany(m => m.Lessons)
             .HasForeignKey(x => x.ModuleId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<LessonProgress>(e =>
        {
            e.ToTable("lesson_progress");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ChildId, x.LessonId }).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Child).WithMany(c => c.Progress).HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Lesson).WithMany(l => l.Progress).HasForeignKey(x => x.LessonId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<MatchmakingQueue>(e =>
        {
            e.ToTable("matchmaking_queue");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChildId).IsUnique();
            e.Property(x => x.JoinedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.Child).WithMany().HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<Duel>(e =>
        {
            e.ToTable("duels");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.Challenger).WithMany().HasForeignKey(x => x.ChallengerChildId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Opponent).WithMany().HasForeignKey(x => x.OpponentChildId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Winner).WithMany().HasForeignKey(x => x.WinnerChildId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        model.Entity<GlucoseReading>(e =>
        {
            e.ToTable("glucose_readings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Value).HasColumnType("numeric(5,2)");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.Child).WithMany().HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Duel).WithMany(d => d.GlucoseReadings).HasForeignKey(x => x.DuelId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
        });

        model.Entity<DuelPointEvent>(e =>
        {
            e.ToTable("duel_point_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason).HasConversion<string>().HasMaxLength(50).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            e.HasOne(x => x.Duel).WithMany(d => d.PointEvents).HasForeignKey(x => x.DuelId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Child).WithMany().HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.GlucoseReading).WithMany().HasForeignKey(x => x.GlucoseReadingId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
        });

        model.Entity<ChildStats>(e =>
        {
            e.ToTable("child_stats");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChildId).IsUnique();
            e.HasOne(x => x.Child).WithMany().HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
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

        SeedData(model);
    }

    private static void SeedData(ModelBuilder model)
    {
        model.Entity<PsychoModule>().HasData(
            new PsychoModule { Id = 1, Title = "Що таке цукровий діабет?", Description = "Основи про діабет для дітей", OrderIndex = 1 },
            new PsychoModule { Id = 2, Title = "Як вимірювати рівень глюкози?", Description = "Навчання користуватись глюкометром", OrderIndex = 2 },
            new PsychoModule { Id = 3, Title = "Правильне харчування", Description = "Хлібні одиниці та збалансована дієта", OrderIndex = 3 },
            new PsychoModule { Id = 4, Title = "Фізична активність", Description = "Спорт і діабет — як поєднати?", OrderIndex = 4 }
        );

        model.Entity<PsychoLesson>().HasData(
            new PsychoLesson { Id = 1,  ModuleId = 1, Title = "Що таке глюкоза?", OrderIndex = 1 },
            new PsychoLesson { Id = 2,  ModuleId = 1, Title = "Типи діабету", OrderIndex = 2 },
            new PsychoLesson { Id = 3,  ModuleId = 1, Title = "Як працює інсулін?", OrderIndex = 3 },
            new PsychoLesson { Id = 4,  ModuleId = 2, Title = "Що таке глюкометр?", OrderIndex = 1 },
            new PsychoLesson { Id = 5,  ModuleId = 2, Title = "Коли вимірювати глюкозу?", OrderIndex = 2 },
            new PsychoLesson { Id = 6,  ModuleId = 2, Title = "Норми рівня глюкози", OrderIndex = 3 },
            new PsychoLesson { Id = 7,  ModuleId = 3, Title = "Що таке хлібна одиниця?", OrderIndex = 1 },
            new PsychoLesson { Id = 8,  ModuleId = 3, Title = "Продукти з низьким глікемічним індексом", OrderIndex = 2 },
            new PsychoLesson { Id = 9,  ModuleId = 3, Title = "Як читати етикетки продуктів?", OrderIndex = 3 },
            new PsychoLesson { Id = 10, ModuleId = 4, Title = "Чому спорт корисний при діабеті?", OrderIndex = 1 },
            new PsychoLesson { Id = 11, ModuleId = 4, Title = "Що взяти з собою на тренування?", OrderIndex = 2 },
            new PsychoLesson { Id = 12, ModuleId = 4, Title = "Гіпоглікемія під час активності", OrderIndex = 3 }
        );
    }
}
