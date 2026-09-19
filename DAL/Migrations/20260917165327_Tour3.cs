using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rovaya.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Tour3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourismGuidePlace_TourismGuideCategories_CategoryId",
                table: "TourismGuidePlace");

            migrationBuilder.DropForeignKey(
                name: "FK_TourismGuidePlace_TourismGuidePlace_ParentPlaceId",
                table: "TourismGuidePlace");

            migrationBuilder.DropForeignKey(
                name: "FK_TourismGuidePlaceTranslation_TourismGuidePlace_PlaceId",
                table: "TourismGuidePlaceTranslation");

            migrationBuilder.DropForeignKey(
                name: "FK_TourismGuideTranslations_TourismGuideCategories_CategoryId",
                table: "TourismGuideTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TourismGuideTranslations",
                table: "TourismGuideTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TourismGuidePlaceTranslation",
                table: "TourismGuidePlaceTranslation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TourismGuidePlace",
                table: "TourismGuidePlace");

            migrationBuilder.DropColumn(
                name: "GeneralDescription",
                table: "TourismGuidePlaceTranslation");

            migrationBuilder.DropColumn(
                name: "HighlightsAndAttractions",
                table: "TourismGuidePlaceTranslation");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "TourismGuidePlace");

            migrationBuilder.RenameTable(
                name: "TourismGuideTranslations",
                newName: "TourismGuideCategoryTranslations");

            migrationBuilder.RenameTable(
                name: "TourismGuidePlaceTranslation",
                newName: "TourismGuidePlaceTranslations");

            migrationBuilder.RenameTable(
                name: "TourismGuidePlace",
                newName: "TourismGuidePlaces");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "TourismGuideCategoryTranslations",
                newName: "TourismGuideCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_TourismGuideTranslations_CategoryId",
                table: "TourismGuideCategoryTranslations",
                newName: "IX_TourismGuideCategoryTranslations_TourismGuideCategoryId");

            migrationBuilder.RenameColumn(
                name: "PlaceId",
                table: "TourismGuidePlaceTranslations",
                newName: "TourismGuidePlaceId");

            migrationBuilder.RenameIndex(
                name: "IX_TourismGuidePlaceTranslation_PlaceId",
                table: "TourismGuidePlaceTranslations",
                newName: "IX_TourismGuidePlaceTranslations_TourismGuidePlaceId");

            migrationBuilder.RenameIndex(
                name: "IX_TourismGuidePlace_ParentPlaceId",
                table: "TourismGuidePlaces",
                newName: "IX_TourismGuidePlaces_ParentPlaceId");

            migrationBuilder.RenameIndex(
                name: "IX_TourismGuidePlace_CategoryId",
                table: "TourismGuidePlaces",
                newName: "IX_TourismGuidePlaces_CategoryId");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "TourismGuideCategories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TourismGuidePlaceTranslations",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "TourismGuidePlaces",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "TourismGuidePlaces",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "TourismGuidePlaces",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TourismGuideCategoryTranslations",
                table: "TourismGuideCategoryTranslations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TourismGuidePlaceTranslations",
                table: "TourismGuidePlaceTranslations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TourismGuidePlaces",
                table: "TourismGuidePlaces",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourismGuideCategoryTranslations_TourismGuideCategories_Tou~",
                table: "TourismGuideCategoryTranslations",
                column: "TourismGuideCategoryId",
                principalTable: "TourismGuideCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TourismGuidePlaces_TourismGuideCategories_CategoryId",
                table: "TourismGuidePlaces",
                column: "CategoryId",
                principalTable: "TourismGuideCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TourismGuidePlaces_TourismGuidePlaces_ParentPlaceId",
                table: "TourismGuidePlaces",
                column: "ParentPlaceId",
                principalTable: "TourismGuidePlaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourismGuidePlaceTranslations_TourismGuidePlaces_TourismGui~",
                table: "TourismGuidePlaceTranslations",
                column: "TourismGuidePlaceId",
                principalTable: "TourismGuidePlaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourismGuideCategoryTranslations_TourismGuideCategories_Tou~",
                table: "TourismGuideCategoryTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourismGuidePlaces_TourismGuideCategories_CategoryId",
                table: "TourismGuidePlaces");

            migrationBuilder.DropForeignKey(
                name: "FK_TourismGuidePlaces_TourismGuidePlaces_ParentPlaceId",
                table: "TourismGuidePlaces");

            migrationBuilder.DropForeignKey(
                name: "FK_TourismGuidePlaceTranslations_TourismGuidePlaces_TourismGui~",
                table: "TourismGuidePlaceTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TourismGuidePlaceTranslations",
                table: "TourismGuidePlaceTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TourismGuidePlaces",
                table: "TourismGuidePlaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TourismGuideCategoryTranslations",
                table: "TourismGuideCategoryTranslations");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "TourismGuideCategories");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TourismGuidePlaceTranslations");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "TourismGuidePlaces");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "TourismGuidePlaces");

            migrationBuilder.RenameTable(
                name: "TourismGuidePlaceTranslations",
                newName: "TourismGuidePlaceTranslation");

            migrationBuilder.RenameTable(
                name: "TourismGuidePlaces",
                newName: "TourismGuidePlace");

            migrationBuilder.RenameTable(
                name: "TourismGuideCategoryTranslations",
                newName: "TourismGuideTranslations");

            migrationBuilder.RenameColumn(
                name: "TourismGuidePlaceId",
                table: "TourismGuidePlaceTranslation",
                newName: "PlaceId");

            migrationBuilder.RenameIndex(
                name: "IX_TourismGuidePlaceTranslations_TourismGuidePlaceId",
                table: "TourismGuidePlaceTranslation",
                newName: "IX_TourismGuidePlaceTranslation_PlaceId");

            migrationBuilder.RenameIndex(
                name: "IX_TourismGuidePlaces_ParentPlaceId",
                table: "TourismGuidePlace",
                newName: "IX_TourismGuidePlace_ParentPlaceId");

            migrationBuilder.RenameIndex(
                name: "IX_TourismGuidePlaces_CategoryId",
                table: "TourismGuidePlace",
                newName: "IX_TourismGuidePlace_CategoryId");

            migrationBuilder.RenameColumn(
                name: "TourismGuideCategoryId",
                table: "TourismGuideTranslations",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_TourismGuideCategoryTranslations_TourismGuideCategoryId",
                table: "TourismGuideTranslations",
                newName: "IX_TourismGuideTranslations_CategoryId");

            migrationBuilder.AddColumn<string>(
                name: "GeneralDescription",
                table: "TourismGuidePlaceTranslation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HighlightsAndAttractions",
                table: "TourismGuidePlaceTranslation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "TourismGuidePlace",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "TourismGuidePlace",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TourismGuidePlaceTranslation",
                table: "TourismGuidePlaceTranslation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TourismGuidePlace",
                table: "TourismGuidePlace",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TourismGuideTranslations",
                table: "TourismGuideTranslations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourismGuidePlace_TourismGuideCategories_CategoryId",
                table: "TourismGuidePlace",
                column: "CategoryId",
                principalTable: "TourismGuideCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourismGuidePlace_TourismGuidePlace_ParentPlaceId",
                table: "TourismGuidePlace",
                column: "ParentPlaceId",
                principalTable: "TourismGuidePlace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourismGuidePlaceTranslation_TourismGuidePlace_PlaceId",
                table: "TourismGuidePlaceTranslation",
                column: "PlaceId",
                principalTable: "TourismGuidePlace",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TourismGuideTranslations_TourismGuideCategories_CategoryId",
                table: "TourismGuideTranslations",
                column: "CategoryId",
                principalTable: "TourismGuideCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
