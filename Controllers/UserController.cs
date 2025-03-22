using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contoso.Data;
using Contoso.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly SchoolContext _context;

    public UsersController(SchoolContext context)
    {
        _context = context;
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
