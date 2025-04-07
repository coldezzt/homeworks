using Microsoft.EntityFrameworkCore;

namespace hw1.Query;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();
}