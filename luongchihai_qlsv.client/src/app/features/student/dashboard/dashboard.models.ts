export interface StudentProfile {
  studentId: string;
  studentName: string;
  className: string;
  majorName: string;
  facultyName: string;
  academicStatus: string;
  email: string;
  phoneNumber: string;
}

export interface KpiWidgets {
  cumulativeGpa: number;
  totalAccumulatedCredits: number;
  academicStanding: string;
}

export interface CurrentCourse {
  courseId: string;
  courseName: string;
  credits: number;
  currentScore: string;
  letterGrade: string;
}

export interface SemesterGpa {
  semester: number;
  semesterGpa: number;
}

export interface DashboardData {
  profile: StudentProfile;
  kpis: KpiWidgets;
  courses: CurrentCourse[];
  gpaHistory: SemesterGpa[];
}
