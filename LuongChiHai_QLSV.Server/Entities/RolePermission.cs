using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    public class RolePermission
    {
        public int RoleID { get; set; }

        public string PermissionID { get; set; }

        public virtual Role Role { get; set; } = null!;

        public virtual Permission Permission { get; set; } = null!;
    }
}
