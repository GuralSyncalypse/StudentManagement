export interface Course {
  courseID: string
  courseName: string;
  credits: number;
}

export interface CourseSection {
  sectionID: number;
  courseID: string;
  semester: number;
  classSection: string; // Mặc định thường là "L01", "L02"...
  maxCapacity: number | null; // Kiểu int? ở C# chuyển thành number | null
  status: string | null;      // "Open", "Closed", v.v.

  // --- Navigation Properties (Dữ liệu liên kết) ---

  // Dữ liệu môn học đi kèm (khi Backend dùng .Include(s => s.Course))
  course?: Course;

  // Thay vì bê nguyên mảng Enrollments nặng nề xuống Client, 
  // Backend thường sẽ đếm và trả về số lượng đã đăng ký hiện tại:
  currentEnrollment?: number;
}

/**
 * Thêm một DTO mở rộng nếu bạn cần dùng cho giao diện 
 * Quản lý/Đăng ký học phần của Sinh viên
 */
export interface AvailableCourseSection extends CourseSection {
  isEnrolled?: boolean;  // Đánh dấu sinh viên hiện tại đã bấm đăng ký lớp này chưa
  isFull?: boolean;      // Kiểm tra nhanh xem lớp đã bị đầy sĩ số chưa
}
