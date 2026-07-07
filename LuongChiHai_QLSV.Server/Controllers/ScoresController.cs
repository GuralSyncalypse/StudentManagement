using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Data;
using Microsoft.AspNetCore.Authorization;

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
    public async Task<ActionResult<IEnumerable<Score>>> GetScore()
    {
        return await _context.Scores.ToListAsync();
    }

    // GET: api/Score/5
    [HttpGet("{scoreid}")]
    public async Task<ActionResult<Score>> GetScore(int scoreid)
    {
        var score = await _context.Scores.FindAsync(scoreid);

        if (score == null)
        {
            return NotFound();
        }

        return score;
    }

    // PUT: api/Score/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{scoreid}")]
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
    [HttpPost]
    [Authorize(Policy = "grade:manage")]
    public async Task<IActionResult> SaveScores([FromBody] List<Score> scores)
    {
        // 1. Kiểm tra danh sách trống
        if (scores == null || !scores.Any())
            return BadRequest("Dữ liệu trống.");

        // 2. Lấy ra Mã đăng ký học (EnrollmentID) từ phần tử đầu tiên
        var enrollmentId = scores.First().EnrollmentID;

        // 3. Xóa sạch các điểm cũ của lượt đăng ký này trong DB
        var oldScores = _context.Scores.Where(s => s.EnrollmentID == enrollmentId);
        _context.Scores.RemoveRange(oldScores);

        // 4. Chuẩn hóa dữ liệu mảng mới trước khi lưu
        foreach (var score in scores)
        {
            score.ScoreID = 0; // Đảm bảo DB tự sinh ID mới, tránh xung đột khóa chính
            score.EnrollmentID = enrollmentId; // Đồng bộ an toàn mã đăng ký
            score.Weight /= 100;
        }

        // 5. Nạp toàn bộ mảng mới vào hàng đợi (Chỉ dùng AddRange duy nhất ở đây)
        _context.Scores.AddRange(scores);

        // 6. Thực thi tất cả các lệnh DELETE và INSERT trên trong 1 Transaction duy nhất
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật điểm thành công!" });
    }

    // DELETE: api/Score/5
    [HttpDelete("{scoreid}")]
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