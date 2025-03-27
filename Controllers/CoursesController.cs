using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contoso.Data;
using Contoso.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class CoursesController : ControllerBase
{
    private readonly SchoolContext _context;

    public CoursesController(SchoolContext context)
    {
        _context = context;
    }
    

    // GET: api/Courses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Department)
            .Include(c => c.Teacher)
            .ToListAsync();

        return Ok(courses); // The circular reference handling is done globally
    }

    // GET: api/Courses/5
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

        return Ok(course); // The circular reference handling is done globally
    }

    // POST: api/Courses
    [HttpPost]
    public async Task<ActionResult<Course>> PostCourse(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourse), new { id = course.CourseID }, course);
    }

    // PUT: api/Courses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCourse(int id, Course course)
    {
        if (id != course.CourseID)
        {
            return BadRequest();
        }

        _context.Entry(course).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Courses.Any(c => c.CourseID == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Courses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
