namespace LuongChiHai_QLSV.Server.DTOs.Users
{
    public class UserListDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> RoleNames { get; set; } = new();
    }

    public class UserDetailDto : UserListDto
    {
    }

    public class CreateUserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateUserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class UserStatusRequestDto
    {
        public bool IsActive { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
