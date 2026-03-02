using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UQ.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRetryAndFailedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "MinimalMessages",
                type: "varchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "MinimalMessages",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "MinimalMessages");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "MinimalMessages");
        }
    }
}
