using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Contoso.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNet.Identity;
using Contoso.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddScoped<PasswordHasherService>();  // Register PasswordHasherService

// Add DbContext and configure SQL Lite connection
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlite("Data Source=ContosoUniversity.db;Cache=Shared;Pooling=false")
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()); // You can comment this out if you don't need detailed logging in production

// Add session handling
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
});

// Authentication (cookies)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Ensure this path exists
        options.AccessDeniedPath = "/AccessDenied"; // Ensure this path exists
        options.LogoutPath = "/Logout"; // Ensure this path exists
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// Authorization builder
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Teacher", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("Student", policy => policy.RequireRole("Student"));
});

// Build the app
var app = builder.Build();

// Apply migrations and seed the database (you can comment this out if you don't need automatic migrations/seeding)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<SchoolContext>();

    context.Database.Migrate(); // Ensure auto-migration works
    DbInitializer.Initialize(context); // Ensure initial data is seeded
}
// Configure error handling for production environment
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // Comment this out if you don't need HTTP Strict Transport Security (HSTS) in production
}
else
{
    app.UseDeveloperExceptionPage(); // Enable during development
}

// Use session middleware (you need this for session management)
app.UseSession();

// Use authentication middleware
app.UseAuthentication(); // Uncommented this line for authentication to work properly

// Use routing, static files, and authorization middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();
app.Run();