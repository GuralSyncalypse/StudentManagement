using LuongChiHai_QLSV.Server.Data.Configurations;
using LuongChiHai_QLSV.Server.DTOs.Reports;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LuongChiHai_QLSV.Server.Data
{
    public class SchoolContext : DbContext
    {
        public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<AcademicProfile> AcademicProfiles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }


        // Views
        public DbSet<StudentCourseGradeDto> BangDiemChiTiet { get; set; }
        public DbSet<StudentCompleteProfileView> StudentCompleteProfiles { get; set; }
        public DbSet<StudentCumulativeGpaView> StudentCumulativeGpas { get; set; }
        public DbSet<StudentSemesterGpaView> StudentSemesterGpas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Áp dụng các cấu hình cho việc quản lý thông tin sinh viên
            modelBuilder.ApplyConfiguration(new StudentConfiguration());
            modelBuilder.ApplyConfiguration(new AcademicProfileConfiguration());

            // 1. Cấu hình Khóa phức hợp cho UserRole (UserID, RoleID)
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserID, ur.RoleID });

            // 2. Đảm bảo Unique cho Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // 3. Đảm bảo Unique cho RoleName
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            // Map view bảng điểm
            modelBuilder.Entity<StudentCourseGradeDto>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("v_BangDiemChiTiet");
            });


            // RolePermission
            modelBuilder.Entity<RolePermission>().ToTable("RolePermission");

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleID, rp.PermissionID });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleID);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionID);

            // UserPermission
            modelBuilder.Entity<UserPermission>().ToTable("UserPermission");

            modelBuilder.Entity<UserPermission>()
                .HasKey(up => new { up.UserID, up.PermissionID });

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserID);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionID);



            // 1. Cấu hình Siêu khóa (Alternate Key) trên bảng CourseSection phục vụ liên kết ngoại
            modelBuilder.Entity<CourseSection>()
                .HasAlternateKey(cs => new { cs.SectionID, cs.CourseID, cs.SemesterID });

            // 2. Cấu hình Khóa ngoại tổ hợp trong bảng Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.CourseSection)
                .WithMany(cs => cs.Enrollments)
                .HasForeignKey(e => new { e.SectionID, e.CourseID, e.SemesterID }) // Cột khóa ngoại ở Enrollment

                // ======= THÊM DÒNG NÀY ĐỂ SỬA LỖI =======
                .HasPrincipalKey(cs => new { cs.SectionID, cs.CourseID, cs.SemesterID }) // Chỉ định rõ siêu khóa đích ở CourseSection
                                                                                         // ========================================

                .OnDelete(DeleteBehavior.Cascade);

            // 3. Tạo ràng buộc UNIQUE trên bảng Enrollment chống đăng ký trùng môn trong kỳ
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentID, e.CourseID, e.SemesterID })
                .IsUnique();

            // Tự động tìm tất cả các file có kế thừa IEntityTypeConfiguration trong toàn bộ Project và nạp vào.
            // modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


            // Views
            modelBuilder.Entity<StudentCompleteProfileView>().ToView("v_StudentCompleteProfile").HasNoKey();
            modelBuilder.Entity<StudentCumulativeGpaView>().ToView("v_StudentCumulativeGPA").HasNoKey();
            modelBuilder.Entity<StudentSemesterGpaView>().ToView("v_StudentSemesterGPA").HasNoKey();
        }
    }
}
