using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contoso.Data;
using Contoso.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Configuration;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly SchoolContext _context;
    private readonly IConfiguration _configuration; // For JWT configuration


    public UsersController(SchoolContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Department)  // Include department information
            .ToListAsync();
        return Ok(users);  // Return the users with all fields
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int? id)
    {
        var user = await _context.Users
            .Include(u => u.Department)  // Include department information
            .FirstOrDefaultAsync(u => u.UserID == id);

        if (user == null)
        {
            return NotFound();  // Return 404 if user not found
        }

        return Ok(user);  // Return the user with all fields
    }

    [HttpGet("test")]
    public IActionResult TestHeaders()
    {
        var headers = HttpContext.Request.Headers;
        return Ok(new { Headers = headers });
    }

    // GET: api/users/me
    [HttpGet("me")]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        // Log received headers for debugging
        Console.WriteLine("Headers received:");
        foreach (var header in HttpContext.Request.Headers)
        {
            Console.WriteLine($"{header.Key}: {header.Value}");
        }

        // Extract Authentication header
        if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return Unauthorized("No Authentication header provided.");
        }

        // Ensure the token is correctly formatted
        var token = authorizationHeader.ToString();
        if (!token.StartsWith("Bearer "))
        {
            return Unauthorized("Invalid token format.");
        }

        // Remove "Bearer " prefix
        token = token.Substring(7).Trim();

        // Validate token and extract UserID
        var userId = ValidateToken(token);
        if (userId == null)
        {
            return Unauthorized("Invalid or expired token.");
        }

        // Retrieve user from database
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Set authentication and authorization headers
        Response.Headers.Add("Authentication", $"Bearer {token}");
        Response.Headers.Add("Authorization", $"Bearer {token}");

        return Ok(new
        {
            user.UserID,
            user.FullName,
            user.Email,
            user.Role,
            user.DepartmentID
        });
    }

    private int? ValidateToken(string token)
    {
        try
        {
            var secretKey = _configuration["Authentication:Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("JWT Secret Key is missing from configuration.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = securityKey
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParams, out var validatedToken);

            // Ensure token is a valid JWT
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null; // Invalid token
            }

            // Extract UserID
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
            if (userIdClaim == null)
            {
                return null; // UserID not found in token
            }

            return int.Parse(userIdClaim.Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation error: {ex.Message}");
            return null;
        }
    }


    // POST: api/users
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromForm] User user, [FromForm] IFormFile profileDocument)
    {
        if (profileDocument != null && profileDocument.Length > 0)
        {
            // Ensure file type is valid (for example, only allow images)
            if (!profileDocument.ContentType.StartsWith("image/"))
            {
                return BadRequest("Invalid file type. Only images are allowed.");
            }

            // Generate a unique file name to avoid conflicts
            var fileName = Path.GetFileNameWithoutExtension(profileDocument.FileName);
            var fileExtension = Path.GetExtension(profileDocument.FileName);
            var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{fileExtension}";

            // Process and save the uploaded file
            var filePath = Path.Combine("wwwroot/uploads", uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileDocument.CopyToAsync(stream);
            }
            user.ProfileDocument = $"/uploads/{uniqueFileName}";  // Save the file path in the user object
        }

        // Save user to the database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    // PUT: api/users/5
    [HttpPut("{id?}")]
    public async Task<IActionResult> PutUser(int? id, [FromForm] User user, [FromForm] IFormFile profileDocument)
    {
        if (id != user.UserID)
        {
            return BadRequest("User ID mismatch");
        }

        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound(); // Return 404 if user doesn't exist
        }

        // If a new profile document is uploaded, handle it
        if (profileDocument != null && profileDocument.Length > 0)
        {

            // Generate a unique file name for the new profile document
            var fileName = Path.GetFileNameWithoutExtension(profileDocument.FileName);
            var fileExtension = Path.GetExtension(profileDocument.FileName);
            var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{fileExtension}";

            // Save the file in a specific location
            var filePath = Path.Combine("wwwroot/uploads", uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileDocument.CopyToAsync(stream);
            }

            // Set the new profile document path for the user
            user.ProfileDocument = $"/uploads/{uniqueFileName}";
        }
        else if (existingUser.ProfileDocument != null)
        {
            // If no new file is uploaded, keep the old profile document path
            user.ProfileDocument = existingUser.ProfileDocument;
        }

        // Update the user's basic details
        _context.Entry(existingUser).CurrentValues.SetValues(user);
        await _context.SaveChangesAsync();

        return NoContent(); // Successful update with no content
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int? id)
    {
        var user = await _context.Users.FindAsync(id);  // Find the user by ID
        if (user == null)
        {
            return NotFound();  // Return 404 if user not found
        }

        // Delete the profile document if it exists
        var filePath = Path.Combine("wwwroot", user.ProfileDocument.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);  // Delete old file if exists
        }

        _context.Users.Remove(user);  // Remove user from the database
        await _context.SaveChangesAsync();  // Save changes to the database

        return NoContent();  // Return success with no content
    }
}
