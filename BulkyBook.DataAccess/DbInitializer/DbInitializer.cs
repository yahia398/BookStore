using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {

            // Apply migrations if they are not
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch( Exception ex )
            {

            }
            // Create roles if they are not
            if (!_roleManager.RoleExistsAsync(SD.Role_User_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Individual)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Company)).GetAwaiter().GetResult();

                // Create an admin user as well
                _userManager.CreateAsync(new AppUser()
                {
                    UserName = "admin@admin.com",
                    Name = "Admin",
                    Email = "admin@admin.com",
                    StreetAddress = "adminAddress",
                    State = "adminState",
                    City = "adminCity",
                    PostalCode = "adminCode",
                    PhoneNumber = "000000000000"
                }, "*Admin159*").GetAwaiter().GetResult();

                AppUser user = _db.AppUsers.FirstOrDefault(x => x.Email == "admin@admin.com");

                _userManager.AddToRoleAsync(user, SD.Role_User_Admin).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
