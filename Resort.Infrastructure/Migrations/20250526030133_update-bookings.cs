using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resort.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatebookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Villas_VilaId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_VilaId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "VilaId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_VillaId",
                table: "Bookings",
                column: "VillaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Villas_VillaId",
                table: "Bookings",
                column: "VillaId",
                principalTable: "Villas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Villas_VillaId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_VillaId",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "VilaId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_VilaId",
                table: "Bookings",
                column: "VilaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Villas_VilaId",
                table: "Bookings",
                column: "VilaId",
                principalTable: "Villas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
