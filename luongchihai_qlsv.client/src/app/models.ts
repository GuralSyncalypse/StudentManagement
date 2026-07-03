
// 1. Interface phục vụ cho trang hiển thị DANH SÁCH (Gọn nhẹ, load nhanh)


export interface Course {
  courseID: number;       // Không dùng dấu ? vì đây là Khoá chính (Primary Key)
  courseName: string;
  courseCode: string;
  credits?: number;

}

