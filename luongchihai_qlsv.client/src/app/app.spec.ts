import '@angular/compiler'; // <-- THÊM DÒNG NÀY VÀO TRÊN CÙNG
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { App } from './app';

describe('App', () => {
  let component: App;
  let fixture: ComponentFixture<App>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [App],
      imports: [RouterTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the app component', () => {
    expect(component).toBeTruthy();
  });
});

// Thêm dòng này ở cuối cùng file app.spec.ts để sửa lỗi nhận diện nhầm của trình khởi chạy test
export default [];
