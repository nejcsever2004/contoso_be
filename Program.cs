using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Contoso.Data;
using Microsoft.AspNetCore.Identity;
using Contoso.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// load configuration for appsetting json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Ensure JWT settings are loaded
var jwtSettings = builder.Configuration.GetSection("Authentication:Jwt");
if (string.IsNullOrEmpty(jwtSettings["SecretKey"]))
{
    throw new Exception("JWT Secret Key is missing from configuration.");
}

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();

// Add Razor Pages
builder.Services.AddDistributedMemoryCache(); // Required for session
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

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/AccessDenied";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
    };
});


// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Teacher", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("Student", policy => policy.RequireRole("Student"));
});

// Build the app
var app = builder.Build();

// Apply migrations and seed the database
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

// Use session middleware
app.UseSession();

// Serve static files
app.UseStaticFiles();

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Use routing and HTTPS redirection
app.UseHttpsRedirection();
app.UseRouting();

// Enable CORS
app.UseCors("AllowAll");

// Map controllers and Razor Pages
app.MapControllers();
app.MapRazorPages();

// Run the app
app.Run();
