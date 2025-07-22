using Microsoft.EntityFrameworkCore;
using Golestan.Models;

public class UniversityContext : DbContext
{
    public UniversityContext(DbContextOptions<UniversityContext> options) : base(options) { }

   
}
