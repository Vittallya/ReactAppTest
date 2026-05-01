using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderCity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SenderAddress = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    RecieverCity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RecieverAddress = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Weight = table.Column<decimal>(type: "TEXT", nullable: false),
                    DateOfPicking = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
