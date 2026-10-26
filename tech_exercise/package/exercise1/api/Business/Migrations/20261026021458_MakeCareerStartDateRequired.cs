using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeCareerStartDateRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill any missing career start from the person's earliest duty, rather than
            // letting EF fill NULLs with a 0001-01-01 placeholder default.
            migrationBuilder.Sql(
                """
                UPDATE "AstronautDetail"
                SET "CareerStartDate" = (
                    SELECT MIN("DutyStartDate") FROM "AstronautDuty"
                    WHERE "AstronautDuty"."PersonId" = "AstronautDetail"."PersonId")
                WHERE "CareerStartDate" IS NULL;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CareerStartDate",
                table: "AstronautDetail",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CareerStartDate",
                table: "AstronautDetail",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");
        }
    }
}
