export interface Score {
  scoreID: number;
  enrollmentID: number;
  scoreType: string;
  weight: number;
  scoreValue: number;
}

export interface Enrollment {
  enrollmentID: number;
  studentID: string;
  sectionID: number;
  enrollDate?: Date;

  scores: Score[];
}
