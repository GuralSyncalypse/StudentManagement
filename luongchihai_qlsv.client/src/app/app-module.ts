import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { JwtInterceptor } from './core/interceptors/jwt.interceptor';
import { ErrorInterceptor } from './core/interceptors/error.interceptor'; // 🔥 Import ErrorInterceptor ở đây
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { NgApexchartsModule } from 'ng-apexcharts';

@NgModule({
  declarations: [App, LoginComponent, RegisterComponent],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    NgApexchartsModule,
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),

    // 1. Đăng ký JwtInterceptor của bạn
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true,
    },

    // 🔥 2. ĐĂNG KÝ ERROR_INTERCEPTOR BẰNG ALERT TẠI ĐÂY 👇
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true, // Chạy song song cả 2 bộ chặn
    },
  ],
  bootstrap: [App],
})
export class AppModule {}
