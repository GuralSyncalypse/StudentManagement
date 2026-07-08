export interface CourseDetail {
  studentID: string;
  enrollmentID: number;
  sectionID: number;
  semester: number;
  courseID: string;
  courseName: string;
  credits: number;
  totalScore: number;
  grade: string;
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

export interface StudentSummary {
  studentID: string;
  semester: number;
  totalEnrollment: number;
  coursesDetail: CourseDetail[];
}
