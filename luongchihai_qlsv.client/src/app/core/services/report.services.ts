import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StudentReport } from '../../core/models/report.model'; // Khớp với file model của bạn

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
   * @param semester Học kỳ cần tra cứu (Ví dụ: 1, 2, 3)
   */
  getSemesterSummary(studentId: string, semester: number): Observable<StudentReport> {
    // Khởi tạo HttpParams để map dữ liệu vào đoạn Query String ([FromQuery]) của .NET Controller
    const params = new HttpParams()
      .set('studentId', studentId)
      .set('semester', semester);

    // URL gọi thực tế sẽ có dạng: /api/Reports/semester-summary?studentId=SVxxx&semester=1
    return this.http.get<StudentReport>(`${this.apiUrl}/semester-summary`, { params });
  }

  getMySummary(semester: number): Observable<StudentReport> {
    // Khởi tạo HttpParams để map dữ liệu vào đoạn Query String ([FromQuery]) của .NET Controller
    const params = new HttpParams()
      .set('semester', semester);

    // URL gọi thực tế sẽ có dạng: /api/Reports/summary?studentId=SVxxx&semester=1
    return this.http.get<StudentReport>(`${this.apiUrl}/summary`, { params });
  }
}
