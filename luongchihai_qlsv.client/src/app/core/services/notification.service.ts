import { Injectable, inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private toastr = inject(ToastrService);

  error(message: string, title: string = 'Lỗi'): void {
    this.toastr.error(message, title, { timeOut: 4000, progressBar: true });
  }

  warning(message: string, title: string = 'Cảnh báo'): void {
    this.toastr.warning(message, title, { timeOut: 3000 });
  }

  success(message: string, title: string = 'Thành công'): void {
    this.toastr.success(message, title, { timeOut: 3000 });
  }

  info(message: string, title: string = 'Thông tin'): void {
    this.toastr.info(message, title);
  }
}
