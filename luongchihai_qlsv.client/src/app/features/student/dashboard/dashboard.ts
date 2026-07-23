import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { DecimalPipe, JsonPipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { DashboardData } from './dashboard.models';
import {
  ChartComponent,
  ApexAxisChartSeries,
  ApexNonAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexYAxis,
  ApexTitleSubtitle,
  ApexDataLabels,
  ApexStroke,
  ApexFill,
  ApexLegend,
  ApexTooltip,
  ApexMarkers,
  ApexPlotOptions,
  ApexResponsive,
  ApexGrid,
  ApexAnnotations,
  ApexStates,
  ApexTheme,
  NgApexchartsModule,
} from 'ng-apexcharts';

export type ChartOptions = {
  series?: ApexAxisChartSeries | ApexNonAxisChartSeries;
  chart?: ApexChart;
  xaxis?: ApexXAxis;
  yaxis?: ApexYAxis | ApexYAxis[];
  title?: ApexTitleSubtitle;
  subtitle?: ApexTitleSubtitle;
  dataLabels?: ApexDataLabels;
  stroke?: ApexStroke;
  fill?: ApexFill;
  legend?: ApexLegend;
  tooltip?: ApexTooltip;
  markers?: ApexMarkers;
  plotOptions?: ApexPlotOptions;
  responsive?: ApexResponsive[];
  grid?: ApexGrid;
  annotations?: ApexAnnotations;
  states?: ApexStates;
  theme?: ApexTheme;
  colors?: string[];
  labels?: any;
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DecimalPipe, JsonPipe, NgApexchartsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class StudentDashboardComponent implements OnInit {
  // Thay inject qua constructor bằng inject() function chuẩn Angular v21
  private http = inject(HttpClient);
  
  // Khai báo các Signal quản lý State
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);
  data = signal<DashboardData | null>(null);

  // Computed Signal tự động tính toán class cho trạng thái học vụ
  statusClass = computed(() => {
    const status = this.data()?.profile.academicStatus;
    switch (status) {
      case 'Đang học': return 'px-3 py-1 bg-emerald-50 text-emerald-700 text-xs font-semibold rounded-full border border-emerald-100';
      case 'Bảo lưu': return 'px-3 py-1 bg-amber-50 text-amber-700 text-xs font-semibold rounded-full border border-amber-100';
      default: return 'px-3 py-1 bg-rose-50 text-rose-700 text-xs font-semibold rounded-full border border-rose-100';
    }
  });

  ngOnInit(): void {
    this.fetchDashboardData();
  }

  public chartOptions: Partial<ChartOptions> = {
    series: [
      {
        name: 'GPA Học Kỳ',
        type: 'column', // Hiển thị cột cho từng kỳ
        data: [6.7, 7.3],
      },
      {
        name: 'GPA Tích Lũy (CPA)',
        type: 'line', // Đường xu hướng tích lũy
        data: [6.7, 6.94],
      },
    ],
    chart: {
      height: 350,
      type: 'line',
      toolbar: {
        show: false // Ẩn thanh công cụ download/zoom để giao diện gọn gàng
      }
    },
    stroke: {
      width: [0, 4], // Cột phẳng không viền, đường line dày 4px
      curve: 'smooth', // Đường line uốn cong mềm mại
    },
    title: {
      text: 'Xu Hướng Điểm Số Qua Các Học Kỳ',
      align: 'left',
      style: {
        fontSize: '16px',
        color: '#0f172a' // Màu chữ slate-900 chuyên nghiệp
      }
    },
    dataLabels: {
      enabled: true,
      enabledOnSeries: [1], // Chỉ hiển thị trực tiếp nhãn số trên đường tích lũy (CPA)
    },
    labels: [
      'Học kỳ 1',
      'Học kỳ 2',
    ],
    yaxis: [
      {
        min: 0,
        max: 10, // Giới hạn thang điểm 10
        tickAmount: 5, // Chia trục Y thành các khoảng: 0, 2, 4, 6, 8, 10
        title: {
          text: 'Thang điểm 10',
        },
      }
    ],
    colors: ['#818cf8', '#f43f5e'],
  };

  private initChart(response: any) {
    const historyData = response.gpaHistory || [];
    console.log(historyData);

    this.chartOptions = {
      series: [
        {
          name: 'GPA Học Kỳ',
          type: 'column',
          data: historyData.map((item: any) => item.semesterGpa)
        },
        {
          name: 'GPA Tích Lũy (CPA)',
          type: 'line',
          data: historyData.map((item: any) => item.cumulativeGpa)
        }
      ],
      chart: {
        height: 350,
        type: 'line',
        toolbar: { show: false }
      },
      stroke: {
        width: [0, 4],
        curve: 'smooth'
      },
      title: {
        text: 'Xu Hướng Điểm Số Qua Các Học Kỳ',
        align: 'left',
        style: { fontSize: '16px', color: '#0f172a' }
      },
      dataLabels: {
        enabled: true,
        enabledOnSeries: [1],
      },
      
      labels: historyData.map((item: any) => `${item.semesterDisplayName}`),
      yaxis: [
        {
          min: 0,
          max: 10,
          tickAmount: 5,
          title: { text: 'Thang điểm 10' }
        }
      ],
      colors: ['#818cf8', '#f43f5e']
    };
  }

  private fetchDashboardData(): void {
    // Gọi API từ backend (Đã cấu hình gộp dữ liệu từ 4 View)
    this.http.get<DashboardData>('/api/dashboards/student')
      .subscribe({
        next: (res) => {
          this.data.set(res);
          this.initChart(res);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.error.set('Không thể tải dữ liệu bảng điểm. Vui lòng kiểm tra lại kết nối mạng!');
          this.isLoading.set(false);
        }
      });
  }

  // Hàm helper định hình màu sắc cho điểm chữ (Grade Letter)
  getGradeBadgeClass(grade: string): string {
    const base = 'px-2 py-0.5 rounded text-xs font-bold ';
    if (grade === 'A') return base + 'bg-emerald-100 text-emerald-800';
    if (grade === 'B') return base + 'bg-blue-100 text-blue-800';
    if (grade === 'C') return base + 'bg-amber-100 text-amber-800';
    if (grade === 'D') return base + 'bg-orange-100 text-orange-800';
    if (grade === 'F') return base + 'bg-rose-100 text-rose-800';
    return base + 'bg-slate-100 text-slate-500';
  }
}
