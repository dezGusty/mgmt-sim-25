using ManagementSimulator.Core;
using ManagementSimulator.Database;
using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Infrastructure.Config;
using ManagementSimulator.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// OpenAPI / Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServices();
builder.Services.AddRepositories();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


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

var app = builder.Build();
AppConfig.Init(app.Configuration);

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MGMTSimulatorDbContext>();

    var itDepartment = dbContext.Departments.FirstOrDefault(d => d.Name == "IT");
    if (itDepartment == null)
    {
        itDepartment = new Department { Name = "IT" };
        dbContext.Departments.Add(itDepartment);
        dbContext.SaveChanges();
    }

    var itAdminTitle = dbContext.JobTitles.FirstOrDefault(jt => jt.Name == "ITAdmin" && jt.DepartmentId == itDepartment.Id);
    if (itAdminTitle == null)
    {
        itAdminTitle = new JobTitle
        {
            Name = "ITAdmin",
            DepartmentId = itDepartment.Id,
            Department = itDepartment
        };
        dbContext.JobTitles.Add(itAdminTitle);
        dbContext.SaveChanges();
    }

    var roleNames = new[] { "Admin", "Manager", "Employee" };
    var roles = new List<EmployeeRole>();
    foreach (var roleName in roleNames)
    {
        var role = dbContext.EmployeeRoles.FirstOrDefault(r => r.Rolename == roleName);
        if (role == null)
        {
            role = new EmployeeRole { Rolename = roleName };
            dbContext.EmployeeRoles.Add(role);
            dbContext.SaveChanges();
        }
        roles.Add(role);
    }

    if (!dbContext.Users.Any(u => u.Email == "admin@simulator.com"))
    {
        var adminRole = roles.First(r => r.Rolename == "Admin");

        var adminUser = new User
        {
            FirstName = "admin",
            LastName = "admin",
            Email = "admin@simulator.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            JobTitleId = itAdminTitle.Id,
            Title = itAdminTitle
        };

        dbContext.Users.Add(adminUser);
        dbContext.SaveChanges();

        var roleUser = new EmployeeRoleUser
        {
            UsersId = adminUser.Id,
            RolesId = adminRole.Id,
            Role = adminRole,
            User = adminUser,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.EmployeeRolesUsers.Add(roleUser);
        dbContext.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
