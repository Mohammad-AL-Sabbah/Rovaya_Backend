using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rovaya.DAL.Migrations
{
    /// <inheritdoc />
    public partial class TourismMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TourismGuidePlace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentPlaceId = table.Column<int>(type: "integer", nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    VideoUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourismGuidePlace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourismGuidePlace_TourismGuideCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TourismGuideCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TourismGuidePlace_TourismGuidePlace_ParentPlaceId",
                        column: x => x.ParentPlaceId,
                        principalTable: "TourismGuidePlace",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TourismGuidePlaceTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaceId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GeneralDescription = table.Column<string>(type: "text", nullable: false),
                    HighlightsAndAttractions = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourismGuidePlaceTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourismGuidePlaceTranslation_TourismGuidePlace_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "TourismGuidePlace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourismGuidePlace_CategoryId",
                table: "TourismGuidePlace",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TourismGuidePlace_ParentPlaceId",
                table: "TourismGuidePlace",
                column: "ParentPlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TourismGuidePlaceTranslation_PlaceId",
                table: "TourismGuidePlaceTranslation",
                column: "PlaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourismGuidePlaceTranslation");

            migrationBuilder.DropTable(
                name: "TourismGuidePlace");
        }
    }
}
