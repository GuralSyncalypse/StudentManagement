using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("Permission")]
    public class Permission
    {
        [Key]
        [StringLength(4)]
        public string PermissionID { get; set; } = string.Empty;
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
