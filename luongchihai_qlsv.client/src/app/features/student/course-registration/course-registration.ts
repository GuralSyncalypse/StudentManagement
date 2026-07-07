import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core'; // 1. Import thêm ChangeDetectionStrategy và ChangeDetectorRef
import { CommonModule } from '@angular/common';
import { CourseRegistrationService, CourseSectionDto } from './course-registration.service';

@Component({
  selector: 'app-course-registration',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './course-registration.html',
  styleUrls: ['./course-registration.css'],
  // 2. Kích hoạt chiến lược OnPush: Angular sẽ KHÔNG tự động bắt thay đổi trừ khi có @Input thay đổi hoặc sự kiện từ UI
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseRegistrationComponent implements OnInit {
  sections: CourseSectionDto[] = [];
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  // 3. Inject ChangeDetectorRef vào constructor dưới tên biến 'cdr'
  constructor(
    private registrationService: CourseRegistrationService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadSections();
  }

  loadSections(): void {
    this.isLoading = true;
    this.errorMessage = '';
    // Vì dùng OnPush, ta cần báo cho Angular biết trạng thái isLoading đã đổi sang true
    this.cdr.markForCheck();

    this.registrationService.getAvailableSections().subscribe({
      next: (data) => {
        this.sections = data;
        this.isLoading = false;

        // 4. QUAN TRỌNG: Dữ liệu trả về từ API là bất đồng bộ (Async), 
        // Angular OnPush sẽ không biết để vẽ lại UI. Ta phải ép nó quét bằng gọi cdr.
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải danh sách học phần. Vui lòng thử lại sau!';
        this.isLoading = false;

        // Báo cập nhật UI khi có lỗi xảy ra
        this.cdr.markForCheck();
      }
    });
  }

  onRegister(section: CourseSectionDto): void {
    // 1. Hiển thị hộp thoại xác nhận của trình duyệt
    const isConfirmed = confirm(`Xác nhận đăng ký học phần: "${section.courseName}" (Lớp: ${section.classSection}) không?`);

    // Nếu chọn "Cancel" thì dừng toàn bộ xử lý
    if (!isConfirmed) return;

    // 2. Nếu chọn "OK" thì tiếp tục chạy logic đăng ký như cũ
    this.errorMessage = '';
    this.successMessage = '';
    this.isLoading = true;
    this.cdr.markForCheck();

    this.registrationService.registerCourse(section.sectionID).subscribe({
      next: (response) => {
        this.successMessage = 'Đăng ký học phần thành công!';
        this.loadSections();
      },
      error: (err) => {
        this.errorMessage = err.error || 'Đăng ký học phần thất bại. Vui lòng kiểm tra lại!';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  // Thêm hàm xử lý khi sinh viên bấm nút Hủy đăng ký
  onDrop(section: CourseSectionDto): void {
    const isConfirmed = confirm(`Bạn có chắc muốn HỦY ĐĂNG KÝ môn: "${section.courseName}" không? Hành động này sẽ nhường suất cho sinh viên khác.`);
    if (!isConfirmed) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck(); // Hiển thị trạng thái loading

    this.registrationService.dropCourse(section.sectionID).subscribe({
      next: (response) => {
        this.successMessage = 'Đã hủy đăng ký học phần thành công!';
        this.loadSections(); // Tải lại danh sách để cập nhật lại nút bấm và sĩ số mới
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Hủy đăng ký thất bại. Vui lòng thử lại!';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }
}
