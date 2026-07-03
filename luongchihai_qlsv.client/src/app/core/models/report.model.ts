export interface CourseDetail {
  studentID: string;
  enrollmentID: number;
  sectionID: number;
  semester: number;
  courseID: string;
  courseName: string;
  credits: number;
  totalScore: number;
  result: 'Đỗ' | 'Trượt';
}

export interface StudentReport {
  studentID: string;
  semester: number;
  tongSoMonDaHoc: number;
  soMonDaQua: number;
  soMonTruot: number;
  diemTrungBinhHocKy: number;
  chiTietMonHoc: CourseDetail[];
}
