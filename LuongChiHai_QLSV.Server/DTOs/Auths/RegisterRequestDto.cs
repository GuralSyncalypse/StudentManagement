namespace LuongChiHai_QLSV.Server.DTOs.Auths
{
    public class RegisterRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RoleName { get; set; } = "Student"; // Mặc định tự đăng ký là Student

        // Các thuộc tính dưới đây chỉ cần thiết nếu RoleName = "Student"
        public string? StudentID { get; set; }
        public string? StudentName { get; set; }
        public string? Gender { get; set; }
    }
}
