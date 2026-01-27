using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManufacturingOptimization.ProviderSimulator.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EstimateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PlannedProcessId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Process = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DeclineReason = table.Column<string>(type: "TEXT", nullable: true),
                    ArrivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MotorSpecs_PowerKW = table.Column<double>(type: "REAL", nullable: false),
                    MotorSpecs_AxisHeightMM = table.Column<int>(type: "INTEGER", nullable: false),
                    MotorSpecs_CurrentEfficiency = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MotorSpecs_TargetEfficiency = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MotorSpecs_MalfunctionDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proposals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlannedProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProposalId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlannedProcesses_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessEstimates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProposalId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    QualityScore = table.Column<double>(type: "REAL", nullable: false),
                    EmissionsKgCO2 = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessEstimates_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlannedProcesses_ProposalId",
                table: "PlannedProcesses",
                column: "ProposalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEstimates_ProposalId",
                table: "ProcessEstimates",
                column: "ProposalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlannedProcesses");

            migrationBuilder.DropTable(
                name: "ProcessEstimates");

            migrationBuilder.DropTable(
                name: "Proposals");
        }
    }
}
