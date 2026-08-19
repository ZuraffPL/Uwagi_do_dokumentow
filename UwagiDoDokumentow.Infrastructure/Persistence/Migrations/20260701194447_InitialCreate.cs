using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UwagiDoDokumentow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_types",
                columns: table => new
                {
                    symbol = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_types", x => x.symbol);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    display_name = table.Column<string>(type: "TEXT", nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", nullable: false),
                    is_admin = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    can_add = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    can_edit = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    can_delete = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "activity_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    action_type = table.Column<string>(type: "TEXT", nullable: false),
                    entity_type = table.Column<string>(type: "TEXT", nullable: true),
                    entity_id = table.Column<int>(type: "INTEGER", nullable: true),
                    details = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_log_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    document_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    document_symbol = table.Column<string>(type: "TEXT", nullable: false),
                    document_number = table.Column<string>(type: "TEXT", nullable: false),
                    ordered_by = table.Column<string>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: true),
                    content = table.Column<string>(type: "TEXT", nullable: false),
                    tags = table.Column<string>(type: "TEXT", nullable: true),
                    is_archived = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    created_by_user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    updated_by_user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_notes_document_types_document_symbol",
                        column: x => x.document_symbol,
                        principalTable: "document_types",
                        principalColumn: "symbol",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notes_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notes_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "note_attachments",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    note_id = table.Column<int>(type: "INTEGER", nullable: false),
                    original_file_name = table.Column<string>(type: "TEXT", nullable: false),
                    stored_file_name = table.Column<string>(type: "TEXT", nullable: false),
                    relative_path = table.Column<string>(type: "TEXT", nullable: false),
                    content_type = table.Column<string>(type: "TEXT", nullable: true),
                    extension = table.Column<string>(type: "TEXT", nullable: false),
                    size_bytes = table.Column<long>(type: "INTEGER", nullable: false),
                    uploaded_by_user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_attachments", x => x.id);
                    table.ForeignKey(
                        name: "FK_note_attachments_document_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "document_notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_note_attachments_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "document_types",
                columns: new[] { "symbol", "created_at", "description", "is_active" },
                values: new object[,]
                {
                    { "DZ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "EK", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "FI", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "FO", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "IV", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "KB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "KF", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "KZ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "M1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "M2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "MM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "PI", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "PZ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "RE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "RO", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "RR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "SO", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "UN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true },
                    { "WZ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true }
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_log_created_at",
                table: "activity_log",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_activity_log_user_id",
                table: "activity_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notes_created_by_user_id",
                table: "document_notes",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_notes_document_date",
                table: "document_notes",
                column: "document_date");

            migrationBuilder.CreateIndex(
                name: "ix_document_notes_ordered_by",
                table: "document_notes",
                column: "ordered_by");

            migrationBuilder.CreateIndex(
                name: "ix_document_notes_symbol_number",
                table: "document_notes",
                columns: new[] { "document_symbol", "document_number" });

            migrationBuilder.CreateIndex(
                name: "ix_document_notes_updated_at",
                table: "document_notes",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_document_notes_updated_by_user_id",
                table: "document_notes",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_attachments_note_id",
                table: "note_attachments",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "IX_note_attachments_uploaded_by_user_id",
                table: "note_attachments",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_log");

            migrationBuilder.DropTable(
                name: "note_attachments");

            migrationBuilder.DropTable(
                name: "document_notes");

            migrationBuilder.DropTable(
                name: "document_types");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
