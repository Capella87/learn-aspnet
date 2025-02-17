using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControllerWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class Addgenreandcompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop a table if it already exist.
            //migrationBuilder.DropTable(name: "GameDeveloper");
            //migrationBuilder.DropTable(name: "GamePublisher");
            //migrationBuilder.DropTable(name: "GameGenre");
            //migrationBuilder.DropTable(name: "Companies");
            //migrationBuilder.DropTable(name: "Genres");

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            // Convert string array typed data to new entities.
            // Insert genres extracted from Games
            migrationBuilder.Sql(@"
                INSERT INTO Genres (Name)
                SELECT DISTINCT TRIM(value) AS 'Name'
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
                SELECT DISTINCT TRIM(value) AS 'Name'
                FROM Games
                CROSS APPLY STRING_SPLIT(Games.Developer, ',');
            ");
            migrationBuilder.Sql(@"
                INSERT INTO Companies (Name)
                SELECT DISTINCT TRIM(value)
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

            // Finally, Execute the drop column command.
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

            // Move the name entity of Genres
            migrationBuilder


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
