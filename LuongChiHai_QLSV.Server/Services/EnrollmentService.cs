using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.DTOs.Students;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Interfaces;

namespace LuongChiHai_QLSV.Server.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnrollmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<EnrollmentDto>> GetMyEnrollmentsAsync(string studentId)
        {
            var enrollments = await _unitOfWork.Enrollments.GetByStudentWithDetailsAsync(studentId);
            return enrollments.Select(MapEnrollmentDto).ToList();
        }

        public async Task<List<EnrollmentDto>> GetAllEnrollmentsAsync()
        {
            var enrollments = await _unitOfWork.Enrollments.GetAllWithDetailsAsync();
            return enrollments.Select(MapEnrollmentDto).ToList();
        }

        public async Task<EnrollmentDto?> GetEnrollmentAsync(int id)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdWithDetailsAsync(id);
            return enrollment == null ? null : MapEnrollmentDto(enrollment);
        }

        public async Task<EnrollmentDto> RegisterCourseAsync(string studentId, StudentRegistrationDto dto)
        {
            return await RegisterInternalAsync(studentId, dto.SectionID);
        }

        public async Task<EnrollmentDto> RegisterForStudentAsync(AdminRegistrationDto dto)
        {
            return await RegisterInternalAsync(dto.StudentID, dto.SectionID);
        }

        // CHỈNH SỬA HÀM UPDATE: Nếu đổi lớp (SectionID), phải cập nhật lại cả CourseID và SemesterID mới
        public async Task<EnrollmentDto> UpdateEnrollmentAsync(int enrollmentId, Enrollment enrollment)
        {
            var existingEnrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
            if (existingEnrollment == null)
            {
                throw new KeyNotFoundException("Đăng ký học phần không tồn tại.");
            }

            if (existingEnrollment.SectionID != enrollment.SectionID)
            {
                var newSection = await _unitOfWork.CourseSections.FirstOrDefaultAsync(s => s.SectionID == enrollment.SectionID);
                if (newSection == null)
                {
                    throw new KeyNotFoundException("Lớp học phần mới chọn không tồn tại.");
                }

                existingEnrollment.SectionID = enrollment.SectionID;
                existingEnrollment.CourseID = newSection.CourseID;       // Cập nhật lại CourseID đồng bộ
                existingEnrollment.SemesterID = newSection.SemesterID;   // Cập nhật lại SemesterID đồng bộ
            }

            existingEnrollment.StudentID = enrollment.StudentID;
            existingEnrollment.EnrollDate = enrollment.EnrollDate;

            _unitOfWork.Enrollments.Update(existingEnrollment);
            await _unitOfWork.CompleteAsync();

            return MapEnrollmentDto(existingEnrollment);
        }

        public async Task DeleteEnrollmentAsync(int id)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);
            if (enrollment == null)
            {
                throw new KeyNotFoundException("Đăng ký học phần không tồn tại.");
            }

            _unitOfWork.Enrollments.Delete(enrollment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task CancelCourseAsync(string studentId, int sectionId)
        {
            await CancelInternalAsync(sectionId, studentId);
        }

        public async Task AdminCancelEnrollmentAsync(int sectionId, string studentId)
        {
            await CancelInternalAsync(sectionId, studentId);
        }

        public async Task<int> GetCurrentEnrollmentCountAsync(string studentId)
        {
            var enrollments = await _unitOfWork.Enrollments.GetByStudentAsync(studentId);
            return enrollments.Count;
        }

        // CHỈNH SỬA HÀM ĐĂNG KÝ INTERNAL: Gán tự động CourseID và SemesterID từ Section tìm được
        private async Task<EnrollmentDto> RegisterInternalAsync(string studentId, int sectionId)
        {
            if (sectionId <= 0 || string.IsNullOrWhiteSpace(studentId))
            {
                throw new BusinessException("Dữ liệu đầu vào không hợp lệ.");
            }

            var section = await _unitOfWork.CourseSections.FirstOrDefaultAsync(s => s.SectionID == sectionId);
            if (section == null)
            {
                throw new KeyNotFoundException("Lớp học phần không tồn tại.");
            }

            if (!string.Equals(section.Status, "Open", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("Lớp học phần này đã đóng.");
            }

            var currentEnrollment = await _unitOfWork.Enrollments.CountBySectionAsync(sectionId);
            if (currentEnrollment >= section.MaxCapacity)
            {
                throw new BusinessException("Lớp học phần đã đủ sĩ số tối đa.");
            }

            var studentExists = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (studentExists == null)
            {
                throw new KeyNotFoundException("Mã số sinh viên không tồn tại.");
            }

            var alreadyRegistered = await _unitOfWork.Enrollments.ExistsAsync(sectionId, studentId);
            if (alreadyRegistered)
            {
                throw new BusinessException("Sinh viên đã đăng ký lớp học phần này.");
            }

            // ĐIỀU CHỈNH TẠI ĐÂY: Tạo thực thể Enrollment với đầy đủ dữ liệu tổ hợp khóa ngoại
            var enrollment = new Enrollment
            {
                SectionID = sectionId,
                StudentID = studentId,
                CourseID = section.CourseID,       // Lấy tự động từ dữ liệu lớp học phần
                SemesterID = section.SemesterID,   // Lấy tự động từ dữ liệu lớp học phần
                EnrollDate = DateTime.UtcNow
            };

            _unitOfWork.Enrollments.Add(enrollment);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                // Bắt exception từ Db Unique Constraint nếu DB chặn trùng lặp môn trong kỳ
                throw new BusinessException("Sinh viên đã đăng ký một lớp học khác của môn này trong cùng học kỳ.");
            }

            return MapEnrollmentDto(enrollment);
        }

        private async Task CancelInternalAsync(int sectionId, string studentId)
        {
            if (sectionId <= 0 || string.IsNullOrWhiteSpace(studentId))
            {
                throw new BusinessException("Dữ liệu đầu vào không hợp lệ.");
            }

            var enrollment = await _unitOfWork.Enrollments.GetBySectionAndStudentAsync(sectionId, studentId);
            if (enrollment == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đăng ký học phần của sinh viên.");
            }

            var section = await _unitOfWork.CourseSections.FirstOrDefaultAsync(s => s.SectionID == sectionId);
            if (section == null)
            {
                throw new KeyNotFoundException("Lớp học phần không tồn tại.");
            }

            if (!string.Equals(section.Status, "Open", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("Học phần này đã đóng đợt chỉnh sửa, không thể tự ý hủy.");
            }

            _unitOfWork.Enrollments.Delete(enrollment);
            await _unitOfWork.CompleteAsync();
        }

        private static EnrollmentDto MapEnrollmentDto(Enrollment enrollment)
        {
            return new EnrollmentDto
            {
                EnrollmentID = enrollment.EnrollmentID,
                StudentID = enrollment.StudentID,
                SectionID = enrollment.SectionID,
                EnrollDate = enrollment.EnrollDate,
                Scores = enrollment.Scores.Select(s => new ScoreDto
                {
                    ScoreID = s.ScoreID,
                    ScoreType = s.ScoreType,
                    Weight = s.Weight,
                    ScoreValue = s.ScoreValue
                }).ToList()
            };
        }
    }

    [Serializable]
    internal class BusinessException : Exception
    {
        public BusinessException()
        {
        }

        public BusinessException(string? message) : base(message)
        {
        }

        public BusinessException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}