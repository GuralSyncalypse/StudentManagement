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

        public async Task<EnrollmentDto> UpdateEnrollmentAsync(int enrollmentId, Enrollment enrollment)
        {
            var existingEnrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
            if (existingEnrollment == null)
            {
                throw new NotFoundException("Đăng ký học phần không tồn tại.");
            }

            existingEnrollment.StudentID = enrollment.StudentID;
            existingEnrollment.SectionID = enrollment.SectionID;
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
                throw new NotFoundException("Đăng ký học phần không tồn tại.");
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

        private async Task<EnrollmentDto> RegisterInternalAsync(string studentId, int sectionId)
        {
            if (sectionId <= 0 || string.IsNullOrWhiteSpace(studentId))
            {
                throw new BusinessException("Dữ liệu đầu vào không hợp lệ.");
            }

            var section = await _unitOfWork.CourseSections.FirstOrDefaultAsync(s => s.SectionID == sectionId);
            if (section == null)
            {
                throw new NotFoundException("Lớp học phần không tồn tại.");
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
                throw new NotFoundException("Mã số sinh viên không tồn tại.");
            }

            var alreadyRegistered = await _unitOfWork.Enrollments.ExistsAsync(sectionId, studentId);
            if (alreadyRegistered)
            {
                throw new BusinessException("Sinh viên đã đăng ký lớp học phần này.");
            }

            var enrollment = new Enrollment
            {
                SectionID = sectionId,
                StudentID = studentId,
                EnrollDate = DateTime.UtcNow
            };

            _unitOfWork.Enrollments.Add(enrollment);
            await _unitOfWork.CompleteAsync();

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
                throw new NotFoundException("Không tìm thấy đăng ký học phần của sinh viên.");
            }

            var section = await _unitOfWork.CourseSections.FirstOrDefaultAsync(s => s.SectionID == sectionId);
            if (section == null)
            {
                throw new NotFoundException("Lớp học phần không tồn tại.");
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

    [Serializable]
    internal class NotFoundException : Exception
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string? message) : base(message)
        {
        }

        public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
