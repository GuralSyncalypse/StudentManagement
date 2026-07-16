import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayoutComponent } from '../../layouts//admin-layout/admin-layout.component'
import { AdminDashboardComponent } from './dashboard/admin-dashboard.component'
import { StudentList } from './student-list/student-list'
import { StudentFormComponent } from './student-list/form/form.component'
import { CourseList } from './course-list/course-list'
import { CourseFormComponent } from './course-list/form/form-component';
import { EnrollmentList } from './enrollment-list/enrollment-list'
import { Reports } from './student-list/report/reports'
import { CourseSectionListComponent } from './course-section-list/course-section-list.component'
import { UserList } from './user-list/user-list'
import { PermissionMatrixComponent } from './permission-matrix/permission-matrix'
import { UserPermissions } from './user-permissions/user-permissions'

const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'students', component: StudentList },
      { path: 'students/form', component: StudentFormComponent },
      { path: 'students/edit/:id', component: StudentFormComponent },
      { path: 'courses', component: CourseList },
      { path: 'courses/create', component: CourseFormComponent },
      { path: 'courses/edit/:id', component: CourseFormComponent },
      { path: 'courseSections', component: CourseSectionListComponent },
      { path: 'users', component: UserList },
      { path: 'permissions', component: PermissionMatrixComponent },
      { path: 'userPermissions', component: UserPermissions },
      { path: 'enrollments', component: EnrollmentList },
      {
        path: 'students/:studentID/report',
        component: Reports },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes), ReactiveFormsModule],
  exports: [RouterModule]
})

export class AdminRoutingModule { }
