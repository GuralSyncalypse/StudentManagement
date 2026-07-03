using Azure.Core;
using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Auths;
using LuongChiHai_QLSV.Server.DTOs.Students;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.EntityFrameworkCore;

namespace LuongChiHai_QLSV.Server.Services
{
    public class StudentService : IStudentService
    {
        private readonly SchoolContext _context;

        public StudentService(SchoolContext context)
        {
            _context = context;
        }

        // CREATE + AUTO CREATE USER
        public async Task CreateStudentAccountAsync(StudentRequestDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var newUser = new User
                {
                    Username = dto.StudentID,
                    PasswordHash = dto.PhoneNumber, // Thực tế nên dùng BCrypt.Net để HashPassword
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(); // Lưu trước để sinh ra UserID tự động

                // Bước B: Tìm và gán Role cho User
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Student");
                if (role == null)
                {
                    throw new ArgumentNullException(nameof(role), "Vai trò (Role) không được phép để null.");
                }


                var userRole = new UserRole { UserID = newUser.UserID, RoleID = role.RoleID };
                _context.UserRoles.Add(userRole);

                // Bước C: Xử lý tạo thực thể Sinh viên nếu quyền là Student

                var newStudent = new Student
                {
                    StudentID = dto.StudentID!,
                    UserID = newUser.UserID,
                    StudentName = dto.StudentName!,

                    Gender = dto.Gender ?? "Khác",
                    BirthDate = dto.BirthDate,
                    Ethnicity = dto.Ethnicity,
                    Religion = dto.Religion,
                    Nationality = dto.Nationality,
                    BirthPlace = dto.BirthPlace,

                    CitizenID = dto.CitizenID,
                    CitizenIDIssueDate = dto.CitizenIDIssueDate,
                    CitizenIDIssuePlace = dto.CitizenIDIssuePlace,

                    PermanentAddress = dto.PermanentAddress,
                    TemporaryAddress = dto.TemporaryAddress
                };
                _context.Students.Add(newStudent);

                // 🔥 CHỈ SAVE 1 LẦN
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // GET ALL
        public async Task<List<StudentResponseDto>> GetAllAsync()
        {
            var students = await _context.Students
            .Include(s => s.AcademicProfile)
            .ToListAsync();

            if (students == null) return [];

            // 2. Chuyển đổi (Map) thủ công từ Entity sang DTO
            var response = students.Select(s => new StudentResponseDto
            {
                StudentID = s.StudentID,
                StudentName = s.StudentName,
                Gender = s.Gender,
                Ethnicity = s.Ethnicity,
                PermanentAddress = s.PermanentAddress,

                // Map object lồng nhau (AcademicProfile)
                AcademicProfile = s.AcademicProfile == null ? null : new AcademicProfileDto
                {
                    ClassName = s.AcademicProfile.ClassName,
                    FacultyName = s.AcademicProfile.FacultyName,
                    MajorName = s.AcademicProfile.MajorName
                }
            }).ToList();

            return response;
        }

        // GET BY ID
        public async Task<StudentResponseDto?> GetByIdAsync(string id)
        {
            var student = await _context.Students
            .Include(s => s.AcademicProfile)
            .FirstOrDefaultAsync(s => s.StudentID == id);

            if (student == null)
            {
                return null;
            }

            // 2. Map thủ công sang DTO
            var response = new StudentResponseDto
            {
                StudentID = student.StudentID,
                StudentName = student.StudentName,
                Gender = student.Gender,
                Ethnicity = student.Ethnicity,
                PermanentAddress = student.PermanentAddress,

                AcademicProfile = student.AcademicProfile == null ? null : new AcademicProfileDto
                {
                    ClassName = student.AcademicProfile.ClassName,
                    FacultyName = student.AcademicProfile.FacultyName,
                    MajorName = student.AcademicProfile.MajorName
                }
            };

            return response;
        }

        // UPDATE
        public async Task UpdateAsync(string id, StudentRequestDto dto)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên");

            student.StudentName = dto.StudentName;
            student.Gender = dto.Gender;
            student.BirthDate = dto.BirthDate;
            student.Ethnicity = dto.Ethnicity;
            student.Religion = dto.Religion;
            student.Nationality = dto.Nationality;
            student.BirthPlace = dto.BirthPlace;
            student.CitizenID = dto.CitizenID;
            student.CitizenIDIssueDate = dto.CitizenIDIssueDate;
            student.CitizenIDIssuePlace = dto.CitizenIDIssuePlace;
            student.PermanentAddress = dto.PermanentAddress;
            student.TemporaryAddress = dto.TemporaryAddress;

            await _context.SaveChangesAsync();
        }

        // DELETE
        public async Task DeleteAsync(string id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên");

            _context.Students.Remove(student);

            // (optional) xóa luôn user
            var user = await _context.Users.FindAsync(student.UserID);

            if (user != null)
                _context.Users.Remove(user);

            await _context.SaveChangesAsync();
        }
    }
}
