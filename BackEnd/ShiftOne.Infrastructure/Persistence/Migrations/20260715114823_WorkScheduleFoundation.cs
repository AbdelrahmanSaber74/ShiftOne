using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftOne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkScheduleFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Branches_BranchId",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkScheduleId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EarlyLeaveMinutes",
                table: "AttendanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FinalStatus",
                table: "AttendanceRecords",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LateMinutes",
                table: "AttendanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OvertimeMinutes",
                table: "AttendanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ScheduledEndTime",
                table: "AttendanceRecords",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ScheduledStartTime",
                table: "AttendanceRecords",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkScheduleId",
                table: "AttendanceRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkScheduleName",
                table: "AttendanceRecords",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkedMinutes",
                table: "AttendanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkSchedules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkScheduleDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsWorkingDay = table.Column<bool>(type: "bit", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    LateGraceMinutes = table.Column<int>(type: "int", nullable: false),
                    EarlyLeaveGraceMinutes = table.Column<int>(type: "int", nullable: false),
                    MinimumWorkingMinutes = table.Column<int>(type: "int", nullable: false),
                    OvertimeEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkScheduleDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkScheduleDays_WorkSchedules_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalTable: "WorkSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_WorkScheduleId",
                table: "Branches",
                column: "WorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_WorkScheduleId",
                table: "AttendanceRecords",
                column: "WorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkScheduleDays_WorkScheduleId_DayOfWeek",
                table: "WorkScheduleDays",
                columns: new[] { "WorkScheduleId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_CompanyId_IsDefault",
                table: "WorkSchedules",
                columns: new[] { "CompanyId", "IsDefault" },
                unique: true,
                filter: "[IsDefault] = 1 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_CompanyId_Name",
                table: "WorkSchedules",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Branches_BranchId",
                table: "AttendanceRecords",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_WorkSchedules_WorkScheduleId",
                table: "AttendanceRecords",
                column: "WorkScheduleId",
                principalTable: "WorkSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_WorkSchedules_WorkScheduleId",
                table: "Branches",
                column: "WorkScheduleId",
                principalTable: "WorkSchedules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Branches_BranchId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_WorkSchedules_WorkScheduleId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_WorkSchedules_WorkScheduleId",
                table: "Branches");

            migrationBuilder.DropTable(
                name: "WorkScheduleDays");

            migrationBuilder.DropTable(
                name: "WorkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Branches_WorkScheduleId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_WorkScheduleId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "WorkScheduleId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "EarlyLeaveMinutes",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "FinalStatus",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "LateMinutes",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "OvertimeMinutes",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ScheduledEndTime",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ScheduledStartTime",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "WorkScheduleId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "WorkScheduleName",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "WorkedMinutes",
                table: "AttendanceRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Branches_BranchId",
                table: "AttendanceRecords",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
