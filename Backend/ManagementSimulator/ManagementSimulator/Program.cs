using ManagementSimulator.Core;
using ManagementSimulator.Database;
using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Infrastructure.Config;
using ManagementSimulator.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddServices();
builder.Services.AddRepositories();


builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/api/auth/login";
        options.Cookie.Name = "ManagementSimulator.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
AppConfig.Init(app.Configuration);

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MGMTSimulatorDbContext>();

    // Seed IT department
    var itDepartment = dbContext.Departments.FirstOrDefault(d => d.Name == "IT");
    if (itDepartment == null)
    {
        itDepartment = new ManagementSimulator.Database.Entities.Department { Name = "IT" };
        dbContext.Departments.Add(itDepartment);
        dbContext.SaveChanges();
    }

    // Seed ITAdmin job title
    var itAdminTitle = dbContext.JobTitles.FirstOrDefault(jt => jt.Name == "ITAdmin" && jt.DepartmentId == itDepartment.Id);
    if (itAdminTitle == null)
    {
        itAdminTitle = new ManagementSimulator.Database.Entities.JobTitle
        {
            Name = "ITAdmin",
            DepartmentId = itDepartment.Id,
            Department = itDepartment
        };
        dbContext.JobTitles.Add(itAdminTitle);
        dbContext.SaveChanges();
    }

    // Seed default roles
    var roleNames = new[] { "Admin", "Manager", "Employee" };
    var roles = new List<ManagementSimulator.Database.Entities.EmployeeRole>();
    foreach (var roleName in roleNames)
    {
        var role = dbContext.EmployeeRoles.FirstOrDefault(r => r.Rolename == roleName);
        if (role == null)
        {
            role = new ManagementSimulator.Database.Entities.EmployeeRole { Rolename = roleName };
            dbContext.EmployeeRoles.Add(role);
            dbContext.SaveChanges();
        }
        roles.Add(role);
    }

    if (!dbContext.Users.Any(u => u.Email == "admin@simulator.com"))
    {
        var adminRole = roles.First(r => r.Rolename == "Admin");

        var adminUser = new ManagementSimulator.Database.Entities.User
        {
            FirstName = "admin",
            LastName = "admin",
            Email = "admin@simulator.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            JobTitleId = itAdminTitle.Id,
            Title = itAdminTitle
        };

        dbContext.Users.Add(adminUser);
        dbContext.SaveChanges(); // Salvează user-ul mai întâi

        // Adaugă relația în tabela de legătură
        var roleUser = new ManagementSimulator.Database.Entities.EmployeeRoleUser
        {
            UsersId = adminUser.Id,
            RolesId = adminRole.Id,
            Role = adminRole,
            User = adminUser,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Add(roleUser);
        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();