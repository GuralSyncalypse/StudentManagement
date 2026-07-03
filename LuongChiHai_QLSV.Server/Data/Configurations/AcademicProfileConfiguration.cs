using LuongChiHai_QLSV.Server.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuongChiHai_QLSV.Server.Data.Configurations
{
    public class AcademicProfileConfiguration : IEntityTypeConfiguration<AcademicProfile>
    {
        public void Configure(EntityTypeBuilder<AcademicProfile> builder)
        {
            // 1. Định nghĩa tên bảng
            builder.ToTable("AcademicProfile");

            // 2. Cấu hình khóa chính
            builder.HasKey(e => e.StudentID);
            builder.Property(e => e.StudentID)
                   .HasColumnType("varchar(15)");

            // 4. Các trường tùy chọn (Cho phép null)
            builder.Property(e => e.AdmissionDate);
            builder.Property(e => e.ClassName).HasMaxLength(50);
            builder.Property(e => e.CampusName).HasMaxLength(100);
            builder.Property(e => e.EducationLevel).HasMaxLength(50);
            builder.Property(e => e.EducationType).HasMaxLength(50);
            builder.Property(e => e.FacultyName).HasMaxLength(100);
            builder.Property(e => e.MajorName).HasMaxLength(100);
            builder.Property(e => e.SpecializationName).HasMaxLength(100);
            builder.Property(e => e.AcademicYear);
            builder.Property(e => e.Status).HasMaxLength(50);

            // 5. Cấu hình quan hệ (1-1 với Student)
            builder.HasOne(a => a.Student)
                   .WithOne(s => s.AcademicProfile)
                   .HasForeignKey<AcademicProfile>(a => a.StudentID)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
