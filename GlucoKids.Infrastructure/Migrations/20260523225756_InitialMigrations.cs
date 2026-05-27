using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GlucoKids.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "achievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IconEmoji = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    XpReward = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirebaseUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "children",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalXp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_children", x => x.Id);
                    table.ForeignKey(
                        name: "FK_children_users_Id",
                        column: x => x.Id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "child_achievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    AchievementId = table.Column<int>(type: "integer", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_child_achievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_child_achievements_achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_child_achievements_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Duel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChallengerChildId = table.Column<int>(type: "integer", nullable: false),
                    OpponentChildId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DuelDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WinnerChildId = table.Column<int>(type: "integer", nullable: true),
                    ChallengerPoints = table.Column<int>(type: "integer", nullable: false),
                    OpponentPoints = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChallengerId = table.Column<int>(type: "integer", nullable: false),
                    OpponentId = table.Column<int>(type: "integer", nullable: false),
                    WinnerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Duel_children_ChallengerId",
                        column: x => x.ChallengerId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Duel_children_OpponentId",
                        column: x => x.OpponentId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Duel_children_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "children",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "health_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    GlucoseMmol = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    MealContext = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InsulinLong = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    InsulinShort = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    CarbohydratesG = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    Mood = table.Column<int>(type: "integer", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_health_records_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_progress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    LessonKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lesson_progress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lesson_progress_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medical_profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    DiabetesType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DiagnosedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    TargetGlucoseMin = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TargetGlucoseMax = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    InsulinBrand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medical_profiles_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "xp_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xp_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_xp_logs_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "glucose_readings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    DuelId = table.Column<int>(type: "integer", nullable: true),
                    Value = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsInRange = table.Column<bool>(type: "boolean", nullable: false),
                    MeasuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_glucose_readings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_glucose_readings_Duel_DuelId",
                        column: x => x.DuelId,
                        principalTable: "Duel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_glucose_readings_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "food_entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HealthRecordId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Calories = table.Column<int>(type: "integer", nullable: false),
                    CarbohydratesG = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    BreadUnits = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    WeightG = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_food_entries_health_records_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalTable: "health_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuelPointEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DuelId = table.Column<int>(type: "integer", nullable: false),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    GlucoseReadingId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelPointEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuelPointEvent_Duel_DuelId",
                        column: x => x.DuelId,
                        principalTable: "Duel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuelPointEvent_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuelPointEvent_glucose_readings_GlucoseReadingId",
                        column: x => x.GlucoseReadingId,
                        principalTable: "glucose_readings",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "achievements",
                columns: new[] { "Id", "Category", "Description", "IconEmoji", "Key", "Title", "XpReward" },
                values: new object[,]
                {
                    { 1, "Lessons", "Заверши свій перший урок", "📖", "first_lesson", "Перший урок", 10 },
                    { 2, "Lessons", "Заверши 5 уроків", "🎓", "lessons_5", "5 уроків", 50 },
                    { 3, "Lessons", "Заверши всі уроки програми", "🏆", "all_lessons", "Всі уроки", 200 },
                    { 4, "Glucose", "Збережи свій перший запис глюкози", "🩸", "first_record", "Перший запис", 10 },
                    { 5, "Glucose", "Збережи 7 записів глюкози", "📊", "records_7", "7 записів", 30 },
                    { 6, "Glucose", "Збережи 30 записів глюкози", "📈", "records_30", "30 записів", 100 },
                    { 7, "Glucose", "5 показників у цільовому діапазоні", "✅", "in_range_5", "У нормі", 50 },
                    { 8, "Battle", "Виграй свій перший батл", "⚔️", "first_win", "Перша перемога", 20 },
                    { 9, "Battle", "Виграй 5 батлів", "🥇", "wins_5", "5 перемог", 75 },
                    { 10, "Battle", "Виграй 10 батлів", "👑", "wins_10", "10 перемог", 150 },
                    { 11, "Streak", "Роби записи 3 дні підряд", "🔥", "streak_3", "3 дні поспіль", 20 },
                    { 12, "Streak", "Роби записи 7 днів підряд", "⚡", "streak_7", "Тижневий стрік", 50 },
                    { 13, "Streak", "Роби записи 30 днів підряд", "💎", "streak_30", "Місячний стрік", 200 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_achievements_Key",
                table: "achievements",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_child_achievements_AchievementId",
                table: "child_achievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_child_achievements_ChildId_AchievementId",
                table: "child_achievements",
                columns: new[] { "ChildId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Duel_ChallengerId",
                table: "Duel",
                column: "ChallengerId");

            migrationBuilder.CreateIndex(
                name: "IX_Duel_OpponentId",
                table: "Duel",
                column: "OpponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Duel_WinnerId",
                table: "Duel",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelPointEvent_ChildId",
                table: "DuelPointEvent",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelPointEvent_DuelId",
                table: "DuelPointEvent",
                column: "DuelId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelPointEvent_GlucoseReadingId",
                table: "DuelPointEvent",
                column: "GlucoseReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_food_entries_HealthRecordId",
                table: "food_entries",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_glucose_readings_ChildId",
                table: "glucose_readings",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_glucose_readings_DuelId",
                table: "glucose_readings",
                column: "DuelId");

            migrationBuilder.CreateIndex(
                name: "IX_health_records_ChildId",
                table: "health_records",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_progress_ChildId_LessonKey",
                table: "lesson_progress",
                columns: new[] { "ChildId", "LessonKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medical_profiles_ChildId",
                table: "medical_profiles",
                column: "ChildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_FirebaseUid",
                table: "users",
                column: "FirebaseUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_xp_logs_ChildId",
                table: "xp_logs",
                column: "ChildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "child_achievements");

            migrationBuilder.DropTable(
                name: "DuelPointEvent");

            migrationBuilder.DropTable(
                name: "food_entries");

            migrationBuilder.DropTable(
                name: "lesson_progress");

            migrationBuilder.DropTable(
                name: "medical_profiles");

            migrationBuilder.DropTable(
                name: "xp_logs");

            migrationBuilder.DropTable(
                name: "achievements");

            migrationBuilder.DropTable(
                name: "glucose_readings");

            migrationBuilder.DropTable(
                name: "health_records");

            migrationBuilder.DropTable(
                name: "Duel");

            migrationBuilder.DropTable(
                name: "children");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
