using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuongChiHai_QLSV.Server.Entities;

namespace LuongChiHai_QLSV.Server.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            // 1. Tên bảng và Khóa chính
            builder.ToTable("Student");
            builder.HasKey(s => s.StudentID);

            // 2. Cấu hình thuộc tính
            builder.Property(s => s.StudentID).HasMaxLength(15);
            builder.Property(s => s.StudentName).IsRequired().HasMaxLength(100);
            builder.Property(s => s.Gender).HasMaxLength(10);
            builder.Property(s => s.Ethnicity).HasMaxLength(30).HasDefaultValue("Kinh");
            builder.Property(s => s.Religion).HasMaxLength(50).HasDefaultValue("Không");
            builder.Property(s => s.Nationality).HasMaxLength(50).HasDefaultValue("Việt Nam");
            builder.Property(s => s.BirthPlace).HasMaxLength(100);
            builder.Property(s => s.CitizenID).HasMaxLength(12); // Char(12)
            builder.Property(s => s.PermanentAddress).HasMaxLength(255);
            builder.Property(s => s.TemporaryAddress).HasMaxLength(255);

            // 3. Cấu hình mối quan hệ 1-1 với User
            builder.HasOne(s => s.User)
                   .WithOne() // Nếu User không có collection Enrollments
                   .HasForeignKey<Student>(s => s.UserID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.AcademicProfile)
                   .WithOne()
                   .HasForeignKey<AcademicProfile>(s => s.StudentID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student) // <--- Điền 'e => e.Student' vào đây (e đại diện cho Enrollment)
            .HasForeignKey(e => e.StudentID) // Chỗ này nên dùng ký hiệu 'e' cho đúng ngữ cảnh của bảng Enrollment
            .OnDelete(DeleteBehavior.Cascade);


            // Đảm bảo UserID là duy nhất trong bảng Student để duy trì 1-1
            builder.HasIndex(s => s.UserID).IsUnique();
        }
    }
}
