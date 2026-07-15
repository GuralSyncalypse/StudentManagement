using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    public class UserPermission
    {
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PermissionID")]
        public string PermissionID { get; set; } = null!;
        public bool IsAllowed { get; set; }

        public virtual Permission Permission { get; set; } = null!;
    }
}
