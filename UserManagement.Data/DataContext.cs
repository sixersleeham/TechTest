using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data;

public class DataContext : DbContext, IDataContext
{
    public DataContext() => Database.EnsureCreated();

    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }


    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseInMemoryDatabase("UserManagement.Data.DataContext");

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>().HasData(new[]
        {
            new User { Id = 1, Forename = "Peter", Surname = "Loew", Email = "ploew@example.com", IsActive = true, DateOfBirth = new DateTime(2004, 12, 03) },
            new User { Id = 2, Forename = "Benjamin Franklin", Surname = "Gates", Email = "bfgates@example.com", IsActive = true, DateOfBirth = new DateTime(1989, 03, 28) },
            new User { Id = 3, Forename = "Castor", Surname = "Troy", Email = "ctroy@example.com", IsActive = false, DateOfBirth = new DateTime(1970, 06, 01) },
            new User { Id = 4, Forename = "Memphis", Surname = "Raines", Email = "mraines@example.com", IsActive = true, DateOfBirth = new DateTime(2000, 01, 11) },
            new User { Id = 5, Forename = "Stanley", Surname = "Goodspeed", Email = "sgodspeed@example.com", IsActive = true, DateOfBirth = new DateTime(1997, 02, 12) },
            new User { Id = 6, Forename = "H.I.", Surname = "McDunnough", Email = "himcdunnough@example.com", IsActive = true, DateOfBirth = new DateTime(1965, 12, 12) },
            new User { Id = 7, Forename = "Cameron", Surname = "Poe", Email = "cpoe@example.com", IsActive = false, DateOfBirth = new DateTime(1988, 08, 17) },
            new User { Id = 8, Forename = "Edward", Surname = "Malus", Email = "emalus@example.com", IsActive = false, DateOfBirth = new DateTime(1977, 09, 21) },
            new User { Id = 9, Forename = "Damon", Surname = "Macready", Email = "dmacready@example.com", IsActive = false, DateOfBirth = new DateTime(2001, 03, 07) },
            new User { Id = 10, Forename = "Johnny", Surname = "Blaze", Email = "jblaze@example.com", IsActive = true, DateOfBirth = new DateTime(1981, 10, 01) },
            new User { Id = 11, Forename = "Robin", Surname = "Feld", Email = "rfeld@example.com", IsActive = true, DateOfBirth = new DateTime(1999, 12, 25) },
        });

        model.Entity<Log>().HasData(new[]
        {
            new Log { Id = 1, UserId = 1, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 2, UserId = 2, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 3, UserId = 3, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 4, UserId = 4, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 5, UserId = 5, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 6, UserId = 6, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 7, UserId = 7, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 8, UserId = 8, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 9, UserId = 9, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 10, UserId = 10, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
            new Log { Id = 11, UserId = 11, Owner = "Admin", Action = "Add", Change = "N/A", TimeStamp = new DateTime(2000, 01, 01) }, 
        });
    }

    public DbSet<User>? Users { get; set; }

    public DbSet<Log>? Logs { get; set; }

    public async Task<List<TEntity>> GetAllAsync<TEntity>() where TEntity : class
    {
        return await base.Set<TEntity>().ToListAsync();
    }

    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class => base.Set<TEntity>();

    public async Task Create<TEntity>(TEntity entity) where TEntity : class
    {
        if(entity == null)
            throw new ArgumentNullException(nameof(entity));
        base.Add(entity);
        await SaveChangesAsync();
    }

    public async new Task Update<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        base.Update(entity);
        await SaveChangesAsync();
    }

    public async Task Delete<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        base.Remove(entity);
        await SaveChangesAsync();
    }
}
