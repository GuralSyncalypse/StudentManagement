export interface StudentCourseGrade {
  studentID: string;
  enrollmentID: number;
  sectionID: number;
  semesterID: number;
  semesterNo: number;
  startYear: number;
  academicYear: string;
  courseID: string;
  courseName: string;
  credits: number;
  totalScore: number;
  grade: string;
  result: string;
}

export interface StudentSummary {
  studentID: string;
  semesterID: number;
  semesterNo: number;
  startYear: number;
  totalEnrollment: number;
  coursesDetail: StudentCourseGrade[];
}
