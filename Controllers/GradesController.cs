using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contoso.Data;
using Contoso.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class GradesController : ControllerBase
{
    private readonly SchoolContext _context;

    public GradesController(SchoolContext context)
    {
        _context = context;
    }

    // GET: api/grades
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Grade>>> GetGrades()
    {
        var grades = await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Course)
            .ToListAsync();
        return Ok(grades);
    }

    // GET: api/grades/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Grade>> GetGrade(int id)
    {
        var grade = await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Course)
            .FirstOrDefaultAsync(g => g.GradeID == id);

        if (grade == null)
        {
            return NotFound();
        }

        return Ok(grade);
    }

    // POST: api/grades
    [HttpPost]
    public async Task<ActionResult<Grade>> PostGrade(Grade grade)
    {
        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGrade), new { id = grade.GradeID }, grade);
    }

    // PUT: api/grades/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutGrade(int id, Grade grade)
    {
        if (id != grade.GradeID)
        {
            return BadRequest("Grade ID mismatch");
        }

        var existingGrade = await _context.Grades.FindAsync(id);
        if (existingGrade == null)
        {
            return NotFound();
        }

        _context.Entry(existingGrade).CurrentValues.SetValues(grade);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/grades/5
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
