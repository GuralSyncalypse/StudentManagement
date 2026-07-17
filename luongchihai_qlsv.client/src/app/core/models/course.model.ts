export interface Course {
  courseID: string;
  courseName: string;
  credits: number;
}

export interface CourseSection {
  sectionID: number;
  courseID: string;
  courseName: string | null; // Đã phẳng hóa trực tiếp ở lớp gốc, không nằm trong object course nữa

  // --- Cấu trúc Học kỳ mới (Thay thế hoàn toàn trường semester cũ) ---
  semesterID: number;
  semesterNo: number;         // Số học kỳ: 1, 2, 3
  startYear: number;          // Năm học bắt đầu: 2026
  academicYear: string;       // Niên khóa dạng chuỗi: "2026-2027"
  semesterDisplayName: string | null; // Tên hiển thị: "Học kỳ I"

  classSection: string;
  maxCapacity: number | null;
  status: string | null;
  currentEnrollment: number;  // Số sinh viên hiện tại đã đăng ký
}

/**
 * Interface mở rộng cho giao diện Đăng ký học phần của Sinh viên
 */
export interface AvailableCourseSection extends CourseSection {
  isEnrolled: boolean;        // Sinh viên đang đăng nhập đã đăng ký môn này chưa
  isFull?: boolean;           // Thuộc tính tính toán thêm ở Frontend (nếu cần)
}
