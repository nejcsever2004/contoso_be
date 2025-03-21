using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contoso.Data;
using Contoso.Models;
using System.Collections.Generic;
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
    public async Task<ActionResult<User>> GetUser(int id)
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
            // Process and save the uploaded file
            var filePath = Path.Combine("wwwroot/uploads", profileDocument.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileDocument.CopyToAsync(stream);
            }
            user.ProfileDocument = $"/uploads/{profileDocument.FileName}";
        }

        // Save user to the database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, User user)
    {
        if (id != user.UserID)
        {
            return BadRequest("User ID mismatch");  // Return error if IDs do not match
        }

        _context.Entry(user).State = EntityState.Modified;  // Set the user entity state to modified

        try
        {
            await _context.SaveChangesAsync();  // Save changes to the database
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Users.Any(u => u.UserID == id))  // Check if the user exists
            {
                return NotFound();  // Return 404 if user does not exist
            }
            throw;  // Rethrow exception if error occurs
        }

        return NoContent();  // Return success with no content
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);  // Find the user by ID
        if (user == null)
        {
            return NotFound();  // Return 404 if user not found
        }

        _context.Users.Remove(user);  // Remove user from the database
        await _context.SaveChangesAsync();  // Save changes to the database

        return NoContent();  // Return success with no content
    }
}
