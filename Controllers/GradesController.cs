using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contoso.Data;
using Contoso.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
public class GradesController : ControllerBase
{
    private readonly SchoolContext _context;

    public GradesController(SchoolContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Department)
            .Include(c => c.Teacher)
            .ToListAsync();

        var jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve, // Handle circular references
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Optionally ignore null properties
        };

        // Serialize directly and return the JSON response without deserializing
        var json = JsonSerializer.Serialize(courses, jsonOptions); // Serialize with circular reference handling
        return Content(json, "application/json"); // Return the serialized JSON as content
    }   

    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourse(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Department)
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.CourseID == id);

        if (course == null)
        {
            return NotFound();
        }

        var jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve // Handle circular references
        };

        var json = JsonSerializer.Serialize(course, jsonOptions); // Serialize with circular reference handling
        return Ok(JsonSerializer.Deserialize<Course>(json)); // Deserialize to preserve the original model
    }


    // POST: api/Grades
    [HttpPost]
    public async Task<ActionResult<Grade>> PostGrade(Grade grade)
    {
        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourse), new { id = grade.GradeID }, grade);
    }

    // PUT: api/Grades/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutGrade(int id, Grade grade)
    {
        if (id != grade.GradeID)
        {
            return BadRequest();
        }

        _context.Entry(grade).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Grades.Any(g => g.GradeID == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Grades/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGrade(int id)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade == null)
        {
            return NotFound();
        }

        _context.Grades.Remove(grade);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
