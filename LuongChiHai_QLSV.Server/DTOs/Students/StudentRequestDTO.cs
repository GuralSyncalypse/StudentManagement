using System.ComponentModel.DataAnnotations;

namespace LuongChiHai_QLSV.Server.DTOs.Students
{
    public class StudentCreateDto : StudentBaseDto
    {
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        [StringLength(15, ErrorMessage = "Mã sinh viên không quá 15 ký tự")]
        public string StudentID { get; set; } = string.Empty;
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        public string Email { get; set; } = string.Empty;
    }


    public class StudentUpdateDto : StudentBaseDto
    {
        // Hiện tại không thêm trường gì, nhưng sau này có thể thêm các trường chỉ cập nhật được (vd: Lý do cập nhật)
    }
}
