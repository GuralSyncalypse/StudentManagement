using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("Role")]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleID { get; set; }

        [Required]
        [StringLength(30)]
        public string RoleName { get; set; } = string.Empty;

        // Quan hệ 1 - Nhiều với bảng trung gian UserRole
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
