using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanFix.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileNationalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "national_id",
                schema: "public",
                table: "profiles",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_national_id",
                schema: "public",
                table: "profiles",
                column: "national_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_profiles_national_id",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "national_id",
                schema: "public",
                table: "profiles");
        }
    }
}
