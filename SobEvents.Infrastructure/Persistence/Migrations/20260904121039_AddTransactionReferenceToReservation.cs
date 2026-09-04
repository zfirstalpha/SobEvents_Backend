using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SobEvents.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionReferenceToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionReference",
                table: "Reservations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TransactionReference",
                table: "Reservations");
        }
    }
}
