import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { StudentReport } from '../../../../core/models/report.model';
import { StudentReportService } from '../../../../core/services/report.services';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, DecimalPipe, FormsModule],
  templateUrl: './reports.html',
  styleUrl: './reports.css',
})
export class Reports implements OnInit {
  public studentID: string = '';
  public selectedSemester: number | null = null;
  public reportData: StudentReport | null = null;
  public isLoading: boolean = false;
  public errorMessage: string | null = null; // Thêm biến hiển thị lỗi nếu không có dữ liệu

  public semesters = [
    { value: 1, label: 'Học kỳ 1' },
    { value: 2, label: 'Học kỳ 2' },
    { value: 3, label: 'Học kỳ 3' }
  ];

  // 2. INJECT THÊM SERVICE VÀO CONSTRUCTOR
  constructor(
    private route: ActivatedRoute,
    private reportService: StudentReportService,
    private location: Location
  ) { }

  ngOnInit(): void {
    const idFromParam = this.route.snapshot.paramMap.get('studentID');
    if (idFromParam) {
      this.studentID = idFromParam;
    }
  }

  // 3. THAY THẾ LOGIC GỌI API THẬT
  onSemesterChange(): void {
    if (!this.selectedSemester || !this.studentID) {
      this.reportData = null;
      this.errorMessage = null;
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    // Gọi đến hàm trong service đã cấu hình HttpParams
    this.reportService.getSemesterSummary(this.studentID, this.selectedSemester).subscribe({
      next: (data) => {
        this.reportData = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Lỗi lấy báo cáo:', err);
        this.reportData = null;
        this.isLoading = false;

        // Bắt lỗi NotFound thực tế từ Backend để báo lên giao diện
        if (err.status === 404) {
          this.errorMessage = err.error || 'Không tìm thấy dữ liệu điểm cho học kỳ này.';
        } else {
          this.errorMessage = 'Có lỗi xảy ra kết nối đến hệ thống máy chủ.';
        }
      }
    });
  }

  goBack(): void {
    this.location.back();
  }
}
