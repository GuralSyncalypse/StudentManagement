import { Component, inject, ChangeDetectorRef, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminStudentService } from './student-list.service';
import { StudentListDto } from '../../../core/models/student.model';

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [RouterModule, FormsModule],
  templateUrl: './student-list.html',
  styleUrl: './student-list.css',
})
export class StudentList implements OnInit {
  private studentService = inject(AdminStudentService);
  private cdr = inject(ChangeDetectorRef);

  allStudents: StudentListDto[] = [];
  students: StudentListDto[] = [];
  facultyOptions: string[] = [];
  searchTerm = '';
  selectedGender = 'All';
  selectedFaculty = 'All';

  // Các thuộc tính phân trang
  currentPage: number = 1;
  totalPages: number = 1;
  pageSize: number = 5; // Mặc định hiển thị ban đầu là 5 dòng

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents() {
    this.studentService.getStudents()
      .subscribe({
        next: (res) => {
          this.allStudents = res;
          this.facultyOptions = this.getUniqueValues(
            res.map(student => student.academicProfile?.facultyName).filter((value): value is string => !!value)
          );
          this.applyFilters();
        },
        error: (err) => {
          console.log(err);
        }
      });
  }

  // --- LOGIC PHÂN TRANG & BỘ LỌC ĐƯỢC CẬP NHẬT TẠI ĐÂY ---

  // 1. Hàm bắt sự kiện chuyển trang (Trước / Sau / Chọn số trang)
  onPageChange(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.applyFilters(); // Chạy lại bộ lọc để cắt mảng dữ liệu cho trang mới
    }
  }

  // 2. Hàm lắng nghe sự thay đổi của bộ chọn số dòng hiển thị (5, 10, 50, 100)
  onPageSizeChange(newPageSize: any) {
    this.pageSize = Number(newPageSize); // Đảm bảo giá trị luôn là kiểu số (number)
    this.currentPage = 1;                // Reset về trang đầu tiên tránh lệch chỉ mục hiển thị
    this.applyFilters();                 // Cập nhật lại giao diện ngay lập tức mà không cần gọi lại API
  }

  onSearch(value: string): void {
    this.searchTerm = value.trim().toLowerCase();
    this.currentPage = 1; // Reset về trang 1 khi tìm kiếm mới
    this.applyFilters();
  }

  onGenderChange(value: string): void {
    this.selectedGender = value;
    this.currentPage = 1; // Reset về trang 1 khi đổi bộ lọc
    this.applyFilters();
  }

  onFacultyChange(value: string): void {
    this.selectedFaculty = value;
    this.currentPage = 1; // Reset về trang 1 khi đổi bộ lọc
    this.applyFilters();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedGender = 'All';
    this.selectedFaculty = 'All';
    this.currentPage = 1;
    this.applyFilters();
  }

  // 3. Hàm xử lý cốt lõi: Lọc -> Tính tổng số trang -> Cắt mảng hiển thị theo trang hiện tại
  applyFilters(): void {
    const normalizedSearch = this.searchTerm;

    // Bước A: Tiến hành lọc dữ liệu dựa trên các điều kiện Tìm kiếm, Giới tính, Khoa
    const filtered = this.allStudents.filter(student => {
      const profile = student.academicProfile;
      const matchSearch = !normalizedSearch || [
        student.studentID,
        student.studentName,
        student.gender,
        student.ethnicity,
        student.permanentAddress,
        profile?.className,
        profile?.facultyName,
        profile?.majorName
      ].some(value => value?.toLowerCase().includes(normalizedSearch));

      const matchGender = this.selectedGender === 'All' || student.gender === this.selectedGender;
      const matchFaculty = this.selectedFaculty === 'All' || profile?.facultyName === this.selectedFaculty;

      return matchSearch && matchGender && matchFaculty;
    });

    // Bước B: Tính toán lại tổng số trang dựa trên mảng dữ liệu ĐÃ LỌC
    this.totalPages = Math.ceil(filtered.length / this.pageSize) || 1;

    // Khống chế trang hiện tại không vượt quá tổng số trang khả dụng
    if (this.currentPage > this.totalPages) {
      this.currentPage = this.totalPages;
    }
    if (this.currentPage < 1) {
      this.currentPage = 1;
    }

    // Bước C: Sử dụng slice() để cắt đúng vùng dữ liệu hiển thị cho trang hiện tại
    const startIndex = (this.currentPage - 1) * this.pageSize;
    this.students = filtered.slice(startIndex, startIndex + this.pageSize);

    this.cdr.detectChanges();
  }

  private getUniqueValues(values: string[]): string[] {
    return [...new Set(values)].sort((a, b) => a.localeCompare(b, 'vi'));
  }

  deleteStudent(id: string) {
    const confirmDelete = confirm('Bạn có chắc chắn muốn xoá sinh viên này không?');

    if (!confirmDelete) return;

    this.studentService.deleteStudent(id)
      .subscribe({
        next: () => {
          this.loadStudents();
        },
        error: (err) => {
          console.log(err);
        }
      });
  }
}
