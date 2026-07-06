using Azure.Core;
using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Auths;
using LuongChiHai_QLSV.Server.DTOs.Students;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LuongChiHai_QLSV.Server.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CREATE + AUTO CREATE USER
        public async Task CreateStudentAccountAsync(StudentRequestDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Bước A: Tìm Role bằng FindAsync (Query trực tiếp DB thay vì lấy hết về)
                var role = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.RoleName == "Student");

                if (role == null)
                {
                    throw new ArgumentNullException(nameof(role), "Vai trò (Role) không tồn tại.");
                }

                // Bước B: Khởi tạo thực thể User
                var newUser = new User
                {
                    Username = dto.StudentID,
                    PasswordHash = dto.PhoneNumber, // Thực tế nên dùng BCrypt.Net để HashPassword
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Bước C: Tận dụng Navigation Property để map quan hệ mà KHÔNG cần SaveChanges trước
                var userRole = new UserRole
                {
                    RoleID = role.RoleID,
                    User = newUser // EF Core tự hiểu và gán UserID sau khi insert User
                };

                var newStudent = new Student
                {
                    StudentID = dto.StudentID!,
                    User = newUser, // EF Core tự map UserID tự động sinh vào đây
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

                // Thêm vào các Repo tương ứng thông qua Unit of Work
                _unitOfWork.Users.Add(newUser);
                _unitOfWork.UserRoles.Add(userRole);
                _unitOfWork.Students.Add(newStudent);

                // 🔥 CHỈ SAVE CHANGES ĐÚNG 1 LẦN DUY NHẤT
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // GET ALL
        public async Task<List<StudentResponseDto>> GetAllAsync()
        {
            // Sử dụng hàm đặc thù có Include dữ liệu AcademicProfile từ StudentRepository
            var students = await _unitOfWork.Students.GetAllWithProfileAsync();

            if (students == null) return [];

            var response = students.Select(s => new StudentResponseDto
            {
                StudentID = s.StudentID,
                StudentName = s.StudentName,
                Gender = s.Gender,
                Ethnicity = s.Ethnicity,
                PermanentAddress = s.PermanentAddress,

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
            // Sử dụng hàm đặc thù lấy Student kèm Profile
            var student = await _unitOfWork.Students.GetByIdWithProfileAsync(id);

            if (student == null) return null;

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
        public async Task<bool> UpdateAsync(string id, StudentRequestDto dto)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);

            if (student == null) return false;

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

            _unitOfWork.Students.Update(student);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        // DELETE
        public async Task<bool> DeleteAsync(string id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);

            if (student == null) return false;

            // Xóa User liên kết trước
            var user = await _unitOfWork.Users.GetByIdAsync(student.UserID);
            if (user != null)
            {
                _unitOfWork.Users.Delete(user);
            }

            // Xóa Student
            _unitOfWork.Students.Delete(student);

            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
