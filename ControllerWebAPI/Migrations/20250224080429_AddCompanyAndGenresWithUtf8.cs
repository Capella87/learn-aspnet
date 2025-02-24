using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControllerWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyAndGenresWithUtf8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase(
                collation: "Korean_100_CI_AS_KS_WS_SC_UTF8");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Games",
                type: "varchar(max)",
                nullable: false,
                collation: "Korean_100_CI_AS_KS_WS_SC_UTF8",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Games",
                type: "varchar(max)",
                nullable: true,
                collation: "Korean_100_CI_AS_KS_WS_SC_UTF8",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Games",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "varchar(max)", nullable: false, collation: "Korean_100_CI_AS_KS_WS_SC_UTF8")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(max)", nullable: false, collation: "Korean_100_CI_AS_KS_WS_SC_UTF8")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameDeveloper",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeveloperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameDeveloper", x => new { x.DeveloperId, x.GameId });
                    table.ForeignKey(
                        name: "FK_GameDeveloper_Companies_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameDeveloper_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GamePublisher",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublisherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePublisher", x => new { x.GameId, x.PublisherId });
                    table.ForeignKey(
                        name: "FK_GamePublisher_Companies_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePublisher_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameGenre",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameGenre", x => new { x.GameId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_GameGenre_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameGenre_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameDeveloper_GameId",
                table: "GameDeveloper",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameGenre_GenreId",
                table: "GameGenre",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePublisher_PublisherId",
                table: "GamePublisher",
                column: "PublisherId");

            // Convert string data to the new entities with raw SQL
            // Convert string array typed data to new entities.
            // Insert genres extracted from Games
            migrationBuilder.Sql(@"
                INSERT INTO Genres (Name)
                SELECT DISTINCT CAST(TRIM(value) AS VARCHAR(MAX)) AS 'Name'
                FROM Games
                CROSS APPLY STRING_SPLIT(Games.Genres, ',');
            ");
            migrationBuilder.Sql(@"
                INSERT INTO GameGenre (GameId, GenreId)
                SELECT gg.Gid, Genres.Id
                FROM Genres
                INNER JOIN (SELECT Games.Id AS 'Gid', TRIM(value) AS 'GenreName'
                            FROM Games
                            CROSS APPLY STRING_SPLIT(Games.Genres, ',')) AS gg
                ON Genres.Name = gg.GenreName;
            ");

            // Convert developers and publishers
            migrationBuilder.Sql(@"
                INSERT INTO Companies (Name)
                SELECT DISTINCT CAST(TRIM(value) AS VARCHAR(MAX)) AS 'Name'
                FROM Games
                CROSS APPLY STRING_SPLIT(Games.Developer, ',');
            ");
            migrationBuilder.Sql(@"
                INSERT INTO Companies (Name)
                SELECT DISTINCT CAST(TRIM(value) AS VARCHAR(MAX))
                FROM Games
                CROSS APPLY STRING_SPLIT(Games.Publisher, ',') AS p
                WHERE TRIM(value) NOT IN (SELECT Name FROM Companies);
            ");
            migrationBuilder.Sql(@"
                INSERT INTO GameDeveloper (GameId, DeveloperId)
                SELECT gd.Id, c.Id
                FROM Companies as c
                INNER JOIN (SELECT Games.Id, TRIM(value) AS 'CompanyName'
                            FROM Games
                            CROSS APPLY STRING_SPLIT(Games.Developer, ',')) AS gd
                ON c.Name = gd.CompanyName;
            ");
            migrationBuilder.Sql(@"
                INSERT INTO GamePublisher (GameId, PublisherId)
                SELECT gp.Id, c.Id
                FROM Companies as c
                INNER JOIN (SELECT Games.Id, TRIM(value) AS 'CompanyName'
                            FROM Games
                            CROSS APPLY STRING_SPLIT(Games.Publisher, ',')) AS gp
                ON c.Name = gp.CompanyName;
            ");

            migrationBuilder.DropColumn(
                name: "Developer",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Genres",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "Games");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Developer",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Genres",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(@"
            WITH GameDevelopersConcat AS (
                SELECT g.Id AS GameId, CAST(STRING_AGG(c.Name, ',') AS NVARCHAR(MAX)) AS Developers

                FROM GameDeveloper gd
                INNER JOIN Companies c ON gd.DeveloperId = c.Id
                INNER JOIN Games g ON gd.GameId = g.Id
                GROUP BY g.Id
            )
            UPDATE g
            SET g.Developer = gdc.Developers
            FROM Games g
            INNER JOIN GameDevelopersConcat gdc ON g.Id = gdc.GameId;

            WITH GamePublishersConcat AS (
                SELECT g.Id AS GameId, CAST(STRING_AGG(c.Name, ',') AS NVARCHAR(MAX)) AS Publishers

                FROM GamePublisher gp
                INNER JOIN Companies c ON gp.PublisherId = c.Id
                INNER JOIN Games g ON gp.GameId = g.Id
                GROUP BY g.Id
            )
            UPDATE g
            SET g.Publisher = gpc.Publishers
            FROM Games g
            INNER JOIN GamePublishersConcat gpc ON g.Id = gpc.GameId;

            WITH GenresConcat AS (
                SELECT g.Id AS GameId, CAST(STRING_AGG(gg.Name, ',') AS NVARCHAR(MAX)) AS Genres

                FROM GameGenre ggr
                INNER JOIN Genres gg ON ggr.GenreId = gg.Id
                INNER JOIN Games g ON ggr.GameId = g.Id
                GROUP BY g.Id
            )
            UPDATE g
            SET g.Genres = gc.Genres
            FROM Games g
            INNER JOIN GenresConcat gc ON g.Id = gc.GameId;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Games",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.DropTable(
                name: "GameDeveloper");

            migrationBuilder.DropTable(
                name: "GameGenre");

            migrationBuilder.DropTable(
                name: "GamePublisher");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
