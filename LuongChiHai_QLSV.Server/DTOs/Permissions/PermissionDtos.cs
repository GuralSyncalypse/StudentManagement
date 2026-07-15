namespace LuongChiHai_QLSV.Server.DTOs.Permissions
{
    public class PermissionUserOptionDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> RoleNames { get; set; } = new();
    }

    public class PermissionMatrixCellDto
    {
        public int PermissionID { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRoleGranted { get; set; }
        public bool? IsDirectOverrideAllowed { get; set; }
        public bool IsEffectiveAllowed { get; set; }
    }

    public class PermissionMatrixRowDto
    {
        public string Resource { get; set; } = string.Empty;
        public string ResourceLabel { get; set; } = string.Empty;
        public int PermissionCount { get; set; }
        public List<PermissionMatrixCellDto> Cells { get; set; } = new();
    }

    public class PermissionMatrixResponseDto
    {
        public List<PermissionUserOptionDto> Users { get; set; } = new();
        public int SelectedUserID { get; set; }
        public List<string> ActionColumns { get; set; } = new();
        public List<PermissionMatrixRowDto> Rows { get; set; } = new();
    }

    public class SavePermissionMatrixRequestDto
    {
        public List<string> SelectedPermissionKeys { get; set; } = new();
    }

    public class UpdateUserPermissionsDto
    {
        public int UserID { get; set; }
        public List<PermissionStatusDto> Permissions { get; set; } = new();
    }

    public class PermissionStatusDto
    {
        public string PermissionID { get; set; } = null!;
        public bool IsAssigned { get; set; }
        public bool IsAllowed { get; set; }
    }
}
