import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { StudentLayoutComponent } from '../../layouts/student-layout/student-layout.component'
import { Dashboard } from './dashboard/dashboard';
import { StudentCoursesComponent } from './my-courses/my-courses.component'
import { StudentProfileComponent } from './profile/profile'
import { CourseRegistrationComponent } from './course-registration/course-registration'

const routes: Routes = [
  {
    path: '',
    component: StudentLayoutComponent,
    children: [
      { path: 'dashboard', component: Dashboard },
      { path: 'my-courses', component: StudentCoursesComponent },
      { path: 'me', component: StudentProfileComponent },
      { path: 'course-registration', component: CourseRegistrationComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes), ReactiveFormsModule],
  exports: [RouterModule]
})

export class StudentRoutingModule { }
