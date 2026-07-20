// ==========================================
// 1. CORE ENTITIES (Mô hình dữ liệu gốc)
// ==========================================

/**
 * Môn học
 */
export interface Course {
  courseID: string;
  courseName: string;
  credits: number;
}

/**
 * Học kỳ
 */
export interface Semester {
  semesterID: number;
  semesterNo: number;
  startYear: number;
  startDate: string;
  endDate: string;
  registrationStartDate: string;
  registrationEndDate: string;
  isRegistrationEnabled: boolean;
  courseSections?: CourseSection[];
}

/**
 * Lớp học phần (Đã phẳng hóa thông tin môn học & học kỳ)
 */
export interface CourseSection {
  sectionID: number;
  courseID: string;
  courseName: string | null;
  credits?: number;

  // --- Cấu trúc Học kỳ phẳng ---
  semesterID: number;
  semesterNo: number;                  // Số học kỳ: 1, 2, 3
  startYear: number;                   // Năm học bắt đầu (vd: 2026)
  academicYear: string;                // Niên khóa (vd: "2026-2027")
  semesterDisplayName: string | null;  // Tên hiển thị (vd: "Học kỳ I")

  classSection: string;                // Tên lớp/nhóm (vd: "L01", "L02")
  maxCapacity: number | null;          // Sĩ số tối đa
  status: string | null;               // Trạng thái: "Open" | "Closed"
  currentEnrollment: number;           // Số sinh viên hiện tại đã đăng ký
}

// ==========================================
// 2. DATA TRANSFER OBJECTS (DTOs)
// ==========================================

/**
 * DTO dùng cho thao tác Thêm mới / Cập nhật Lớp học phần
 */
export interface CreateCourseSectionDto {
  courseID: string;
  semesterID: number;
  classSection?: string;               // Mặc định backend là "L01" nếu không truyền
  maxCapacity?: number | null;         // Mặc định null hoặc số nguyên
  status?: string;                     // Ví dụ: 'Open' | 'Closed'
}

// ==========================================
// 3. FRONTEND VIEW MODELS (Mở rộng cho UI)
// ==========================================

/**
 * Interface mở rộng dành riêng cho giao diện Đăng ký học phần của Sinh viên
 */
export interface AvailableCourseSection extends CourseSection {
  isEnrolled: boolean;                 // Sinh viên hiện tại đã đăng ký lớp này chưa
  isFull?: boolean;                    // Thuộc tính tính toán thêm trên FE (currentEnrollment >= maxCapacity)
}
