import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { authGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent, canActivate: [authGuard], data: { expectedRoles: ['Admin'] } },

  // Tuyến đường dành cho ADMIN
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin-routing.module').then(m => m.AdminRoutingModule),
    canActivate: [authGuard],
    data: { expectedRoles: ['Admin'] },
  },
  // PHÂN HỆ ADMIN: Chỉ Admin mới được load module quản lý
  
  // {
  //   path: 'courses',
  //   canActivate: [authGuard],
  //   data: { expectedRoles: ['Admin'] },
  //   loadChildren: () => import('./courses/courses.module').then(m => m.CoursesModule)
  // },

  // PHÂN HỆ STUDENT: Trang xem kết quả riêng cho sinh viên
  // {
  //   path: 'my-results',
  //   canActivate: [authGuard],
  //   data: { expectedRoles: ['Student'] },
  //   loadChildren: () => import('./student-results/student-results.module').then(m => m.StudentResultsModule)
  // },

  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
