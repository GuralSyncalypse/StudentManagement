using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ScoresController : ControllerBase
{
    private readonly SchoolContext _context;
    public ScoresController(SchoolContext context)
    {
        _context = context;
    }

    // GET: api/Score
    [HttpGet]
    [HasPermission("0902")]
    public async Task<ActionResult<IEnumerable<Score>>> GetScore()
    {
        return await _context.Scores.AsNoTracking().ToListAsync();
    }

    // GET: api/Score/5
    [HttpGet("{scoreid}")]
    [HasPermission("0902")]
    public async Task<ActionResult<Score>> GetScore(int scoreid)
    {
        var score = await _context.Scores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ScoreID == scoreid);

        if (score == null)
        {
            return NotFound();
        }

        return score;
    }

    // PUT: api/Score/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{scoreid}")]
    [HasPermission("0903")]
    public async Task<IActionResult> PutScore(int? scoreid, Score score)
    {
        if (scoreid != score.ScoreID)
        {
            return BadRequest();
        }

        _context.Entry(score).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ScoreExists(scoreid))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Score
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // 1. API Tạo lẻ 1 đầu điểm (Dùng khi giảng viên muốn thêm thủ công 1 môn)
    [HttpPost("{enrollmentID}")]
    [HasPermission("0901")]
    public async Task<IActionResult> SaveScores(int enrollmentID, [FromBody] List<Score> scores)
    {
        // 1. Kiểm tra danh sách trống
        if (scores == null || !scores.Any())
            return BadRequest("Dữ liệu trống.");

        // 2. Lấy ra Mã đăng ký học (EnrollmentID) từ phần tử đầu tiên
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 3. Xóa sạch các điểm cũ của lượt đăng ký này trong DB
            var oldScores = await _context.Scores
                .Where(s => s.EnrollmentID == enrollmentID)
                .ToListAsync();
            _context.Scores.RemoveRange(oldScores);

            // 4. Chuẩn hóa dữ liệu mảng mới trước khi lưu
            foreach (var score in scores)
            {
                score.ScoreID = 0; // Đảm bảo DB tự sinh ID mới, tránh xung đột khóa chính
                score.EnrollmentID = enrollmentID; // Đồng bộ an toàn mã đăng ký
            }

            // 5. Nạp toàn bộ mảng mới vào hàng đợi (Chỉ dùng AddRange duy nhất ở đây)
            _context.Scores.AddRange(scores);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Cập nhật điểm thành công!" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Có lỗi xảy ra trong quá trình cập nhật điểm.", error = ex.Message });
        }
    }

    // DELETE: api/Score/5
    [HttpDelete("{scoreid}")]
    [HasPermission("0904")]
    public async Task<IActionResult> DeleteScore(int? scoreid)
    {
        var score = await _context.Scores.FindAsync(scoreid);
        if (score == null)
        {
            return NotFound();
        }

        _context.Scores.Remove(score);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ScoreExists(int? scoreid)
    {
        return _context.Scores.Any(e => e.ScoreID == scoreid);
    }
}
