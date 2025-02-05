using ControllerWebAPI.Models;
using ControllerWebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace ControllerWebAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games { get; set; }
}
