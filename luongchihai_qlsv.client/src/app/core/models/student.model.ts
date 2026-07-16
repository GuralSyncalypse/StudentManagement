// 1. DTO Học bạ rút gọn
export interface AcademicProfileResponseDto {
  className?: string;
  facultyName?: string;
  majorName?: string;
}

// 2. Base DTO chứa các trường cá nhân chung (Dùng để kế thừa)
export interface StudentBaseDto {
  phoneNumber: string;
  email: string;
  studentName: string;
  gender?: string;
  birthDate?: string; // DateTime từ C# map sang chuỗi ISO-8601 ở TS
  ethnicity?: string;
  religion?: string;
  nationality?: string;
  birthPlace?: string;
  citizenID?: string;
  citizenIDIssueDate?: string;
  citizenIDIssuePlace?: string;
  permanentAddress?: string;
  temporaryAddress?: string;
}

// 3. DTO dùng cho màn hình Danh Sách (Siêu nhẹ)
export interface StudentListDto {
  studentID: string;
  studentName: string;
  gender?: string;
  ethnicity?: string;
  permanentAddress?: string;
  academicProfile?: AcademicProfileResponseDto;
}

// 4. DTO dùng cho màn hình Chi Tiết (Kế thừa từ Base + thêm ID và Học bạ)
export interface StudentDetailDto extends StudentBaseDto {
  studentID: string;
  userID: number;
  academicProfile?: AcademicProfileResponseDto;
}
