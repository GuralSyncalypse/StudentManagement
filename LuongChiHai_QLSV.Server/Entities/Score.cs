using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("Score")]
    public class Score
    {
        [Key] // Nếu bạn muốn chỉ định rõ đây là khóa chính
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScoreID { get; set; }

        public int EnrollmentID { get; set; }

        [Required]
        [StringLength(50)]
        public string ScoreType { get; set; } = null!; // 'Chuyên cần', 'Giữa kỳ', 'Cuối kỳ'...

        [Column(TypeName = "decimal(3,2)")]
        public decimal Weight { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? ScoreValue { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(EnrollmentID))]
        [ValidateNever]
        public virtual Enrollment Enrollment { get; set; } = null!;
    }
}
