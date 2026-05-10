using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GlucoKids.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "psycho_modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_psycho_modules", x => x.Id);
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
                name: "psycho_lessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_psycho_lessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_psycho_lessons_psycho_modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "psycho_modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "children",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true)
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
                name: "parent_child_links",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<int>(type: "integer", nullable: false),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    InviteCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LinkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parent_child_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parent_child_links_users_ChildId",
                        column: x => x.ChildId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parent_child_links_users_ParentId",
                        column: x => x.ParentId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "parents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parents_users_Id",
                        column: x => x.Id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "child_stats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    TotalDuels = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    Draws = table.Column<int>(type: "integer", nullable: false),
                    WinStreak = table.Column<int>(type: "integer", nullable: false),
                    BestStreak = table.Column<int>(type: "integer", nullable: false),
                    TotalPoints = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_child_stats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_child_stats_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "duels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChallengerChildId = table.Column<int>(type: "integer", nullable: false),
                    OpponentChildId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DuelDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WinnerChildId = table.Column<int>(type: "integer", nullable: true),
                    ChallengerPoints = table.Column<int>(type: "integer", nullable: false),
                    OpponentPoints = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_duels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_duels_children_ChallengerChildId",
                        column: x => x.ChallengerChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_duels_children_OpponentChildId",
                        column: x => x.OpponentChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_duels_children_WinnerChildId",
                        column: x => x.WinnerChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    LessonId = table.Column<int>(type: "integer", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_lesson_progress_psycho_lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "psycho_lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "matchmaking_queue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matchmaking_queue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_matchmaking_queue_children_ChildId",
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
                        name: "FK_glucose_readings_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_glucose_readings_duels_DuelId",
                        column: x => x.DuelId,
                        principalTable: "duels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "duel_point_events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DuelId = table.Column<int>(type: "integer", nullable: false),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GlucoseReadingId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_duel_point_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_duel_point_events_children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_duel_point_events_duels_DuelId",
                        column: x => x.DuelId,
                        principalTable: "duels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_duel_point_events_glucose_readings_GlucoseReadingId",
                        column: x => x.GlucoseReadingId,
                        principalTable: "glucose_readings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "psycho_modules",
                columns: new[] { "Id", "Description", "OrderIndex", "Title" },
                values: new object[,]
                {
                    { 1, "Основи про діабет для дітей", 1, "Що таке цукровий діабет?" },
                    { 2, "Навчання користуватись глюкометром", 2, "Як вимірювати рівень глюкози?" },
                    { 3, "Хлібні одиниці та збалансована дієта", 3, "Правильне харчування" },
                    { 4, "Спорт і діабет — як поєднати?", 4, "Фізична активність" }
                });

            migrationBuilder.InsertData(
                table: "psycho_lessons",
                columns: new[] { "Id", "Content", "ModuleId", "OrderIndex", "Title" },
                values: new object[,]
                {
                    { 1, null, 1, 1, "Що таке глюкоза?" },
                    { 2, null, 1, 2, "Типи діабету" },
                    { 3, null, 1, 3, "Як працює інсулін?" },
                    { 4, null, 2, 1, "Що таке глюкометр?" },
                    { 5, null, 2, 2, "Коли вимірювати глюкозу?" },
                    { 6, null, 2, 3, "Норми рівня глюкози" },
                    { 7, null, 3, 1, "Що таке хлібна одиниця?" },
                    { 8, null, 3, 2, "Продукти з низьким глікемічним індексом" },
                    { 9, null, 3, 3, "Як читати етикетки продуктів?" },
                    { 10, null, 4, 1, "Чому спорт корисний при діабеті?" },
                    { 11, null, 4, 2, "Що взяти з собою на тренування?" },
                    { 12, null, 4, 3, "Гіпоглікемія під час активності" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_child_stats_ChildId",
                table: "child_stats",
                column: "ChildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_duel_point_events_ChildId",
                table: "duel_point_events",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_duel_point_events_DuelId",
                table: "duel_point_events",
                column: "DuelId");

            migrationBuilder.CreateIndex(
                name: "IX_duel_point_events_GlucoseReadingId",
                table: "duel_point_events",
                column: "GlucoseReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_duels_ChallengerChildId",
                table: "duels",
                column: "ChallengerChildId");

            migrationBuilder.CreateIndex(
                name: "IX_duels_OpponentChildId",
                table: "duels",
                column: "OpponentChildId");

            migrationBuilder.CreateIndex(
                name: "IX_duels_WinnerChildId",
                table: "duels",
                column: "WinnerChildId");

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
                name: "IX_lesson_progress_ChildId_LessonId",
                table: "lesson_progress",
                columns: new[] { "ChildId", "LessonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lesson_progress_LessonId",
                table: "lesson_progress",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_queue_ChildId",
                table: "matchmaking_queue",
                column: "ChildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parent_child_links_ChildId",
                table: "parent_child_links",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_parent_child_links_InviteCode",
                table: "parent_child_links",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parent_child_links_ParentId",
                table: "parent_child_links",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_psycho_lessons_ModuleId",
                table: "psycho_lessons",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_FirebaseUid",
                table: "users",
                column: "FirebaseUid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "child_stats");

            migrationBuilder.DropTable(
                name: "duel_point_events");

            migrationBuilder.DropTable(
                name: "health_records");

            migrationBuilder.DropTable(
                name: "lesson_progress");

            migrationBuilder.DropTable(
                name: "matchmaking_queue");

            migrationBuilder.DropTable(
                name: "parent_child_links");

            migrationBuilder.DropTable(
                name: "parents");

            migrationBuilder.DropTable(
                name: "glucose_readings");

            migrationBuilder.DropTable(
                name: "psycho_lessons");

            migrationBuilder.DropTable(
                name: "duels");

            migrationBuilder.DropTable(
                name: "psycho_modules");

            migrationBuilder.DropTable(
                name: "children");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
