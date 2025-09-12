using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSupportSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFileAttachmentsAndEmailIngestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailIngestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OriginalBody = table.Column<string>(type: "TEXT", nullable: false),
                    ProcessedBody = table.Column<string>(type: "TEXT", nullable: false),
                    FromEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FromName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MessageId = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsProcessed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedTicketId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProcessingError = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailIngestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailIngestions_Tickets_CreatedTicketId",
                        column: x => x.CreatedTicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TicketAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TicketId = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadedById = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DownloadToken = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TokenExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAttachments_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketAttachments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailIngestions_CreatedTicketId",
                table: "EmailIngestions",
                column: "CreatedTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailIngestions_IsProcessed",
                table: "EmailIngestions",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_EmailIngestions_MessageId",
                table: "EmailIngestions",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailIngestions_ReceivedAt",
                table: "EmailIngestions",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_DownloadToken",
                table: "TicketAttachments",
                column: "DownloadToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_TicketId",
                table: "TicketAttachments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_UploadedById",
                table: "TicketAttachments",
                column: "UploadedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailIngestions");

            migrationBuilder.DropTable(
                name: "TicketAttachments");
        }
    }
}
