import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StudentSummary } from '../../core/models/report.model'; // Khớp với file model của bạn

@Injectable({
  providedIn: 'root' // Service Global, có thể inject vào bất kỳ component nào
})
export class StudentReportService {
  // Thay thế bằng đường dẫn API gốc (Base URL) thực tế trong cấu hình môi trường của bạn
  private apiUrl = '/api/Reports';

  constructor(private http: HttpClient) { }

  /**
   * Lấy dữ liệu báo cáo tổng hợp kết quả học tập theo học kỳ của sinh viên
   * @param studentId Mã số sinh viên (Ví dụ: SV20260001)
   * @param semesterId Học kỳ cần tra cứu (Ví dụ: 1, 2, 3)
   */
  getSemesterSummary(studentId: string, semesterId: number): Observable<StudentSummary> {
    // Khởi tạo HttpParams để map dữ liệu vào đoạn Query String ([FromQuery]) của .NET Controller
    const params = new HttpParams()
      .set('studentId', studentId)
      .set('semesterId', semesterId);

    // URL gọi thực tế sẽ có dạng: /api/Reports/semester-summary?studentId=SVxxx&semester=1
    return this.http.get<StudentSummary>(`${this.apiUrl}/semester-summary`, { params });
  }

  getMySummary(semesterNo: number, year: number): Observable<StudentSummary> {
    // Khởi tạo HttpParams để map dữ liệu vào đoạn Query String ([FromQuery]) của .NET Controller
    const params = new HttpParams()
      .set('semesterNo', semesterNo)
      .set('year', year);

    // URL gọi thực tế sẽ có dạng: /api/Reports/summary?studentId=SVxxx&semester=1
    return this.http.get<StudentSummary>(`${this.apiUrl}/summary`, { params });
  }
}
