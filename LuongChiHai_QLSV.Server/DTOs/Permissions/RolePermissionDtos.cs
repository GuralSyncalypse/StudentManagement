namespace LuongChiHai_QLSV.Server.DTOs.Permissions
{
    public class RolePermissionRoleOptionDto
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class RolePermissionMatrixCellDto
    {
        public string PermissionID { get; set; } = string.Empty;
        public string PermissionKey { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }

    public class RolePermissionMatrixRowDto
    {
        public string Resource { get; set; } = string.Empty;
        public string ResourceLabel { get; set; } = string.Empty;
        public int PermissionCount { get; set; }
        public List<RolePermissionMatrixCellDto> Cells { get; set; } = new();
    }

    public class RolePermissionMatrixResponseDto
    {
        public List<RolePermissionRoleOptionDto> Roles { get; set; } = new();
        public int SelectedRoleID { get; set; }
        public List<RolePermissionMatrixRowDto> Rows { get; set; } = new();
    }

    public class SaveRolePermissionMatrixRequestDto
    {
        public List<string> SelectedPermissionIds { get; set; } = new();
    }
}
