using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShop.DataAccess.DbInitializer;

public class DbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }
    
    public void Initialize()
    {
        if (_db.Database.GetPendingMigrations().Any())
        {
            _db.Database.Migrate();
        }

        if (_roleManager.RoleExistsAsync(Roles.Admin.ToString()).GetAwaiter().GetResult()) return;

        _roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString())).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(Roles.Company.ToString())).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(Roles.Employee.ToString())).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(Roles.Individual.ToString())).GetAwaiter().GetResult();

        _userManager.CreateAsync(new ApplicationUser()
        {
            UserName = "admin@bookshop.com",
            Email = "admin@bookshop.com",
            Name = "Admin",
            PhoneNumber = "1112223333",
            StreetAddress = "Admin street",
            State = "Lviv",
            PostalCode = "79012",
            City = "Lviv"
        }, "Admin123*").GetAwaiter().GetResult();

        var user = _db.Users.FirstOrDefault(u => u.Email == "admin@bookshop.com");

        _userManager.AddToRoleAsync(user, Roles.Admin.ToString()).GetAwaiter().GetResult();
    }
}