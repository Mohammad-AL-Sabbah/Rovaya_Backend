using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rovaya.DAL.Migrations
{
    /// <inheritdoc />
    public partial class New : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourismGuidePlaces_TourismGuideCategories_CategoryId",
                table: "TourismGuidePlaces");

            migrationBuilder.DropIndex(
                name: "IX_TourismGuidePlaces_CategoryId",
                table: "TourismGuidePlaces");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "TourismGuidePlaces");

            migrationBuilder.CreateTable(
                name: "TourismGuideCategoryTourismGuidePlace",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "integer", nullable: false),
                    PlacesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourismGuideCategoryTourismGuidePlace", x => new { x.CategoriesId, x.PlacesId });
                    table.ForeignKey(
                        name: "FK_TourismGuideCategoryTourismGuidePlace_TourismGuideCategorie~",
                        column: x => x.CategoriesId,
                        principalTable: "TourismGuideCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourismGuideCategoryTourismGuidePlace_TourismGuidePlaces_Pl~",
                        column: x => x.PlacesId,
                        principalTable: "TourismGuidePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourismGuideCategoryTourismGuidePlace_PlacesId",
                table: "TourismGuideCategoryTourismGuidePlace",
                column: "PlacesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourismGuideCategoryTourismGuidePlace");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "TourismGuidePlaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TourismGuidePlaces_CategoryId",
                table: "TourismGuidePlaces",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_TourismGuidePlaces_TourismGuideCategories_CategoryId",
                table: "TourismGuidePlaces",
                column: "CategoryId",
                principalTable: "TourismGuideCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
