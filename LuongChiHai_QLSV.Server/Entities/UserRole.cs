using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("UserRole")]
    public class UserRole
    {
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;

        public int RoleID { get; set; }
        [ForeignKey("RoleID")]
        public virtual Role Role { get; set; } = null!;
    }
}
