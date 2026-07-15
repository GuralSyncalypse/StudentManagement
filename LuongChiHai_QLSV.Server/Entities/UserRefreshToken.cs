using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("UserRefreshToken")]
    public class UserRefreshToken
    {
        [Key]
        public int ID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(512)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(45)]
        public string? CreatedByIp { get; set; }


        [StringLength(255)]
        public string? Device { get; set; } // Tên thiết bị dễ đọc: e.g. "Chrome (Windows)"

        public string? UserAgent { get; set; } // Lưu chuỗi User-Agent gốc để đối chiếu khi cần

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        // Navigation property liên kết tới bảng User
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }
}
