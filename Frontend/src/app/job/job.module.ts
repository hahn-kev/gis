import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { JobRoutingModule } from './job-routing.module';
import { JobComponent } from './job/job.component';
import { JobListComponent } from './list/job-list.component';
import { GradeComponent } from './grade/edit/grade.component';
import { GradeListComponent } from './grade/list/grade-list.component';
import { RolesReportComponent } from '../people/roles-report/roles-report.component';
import { AppComponentsModule } from '../components/app-components.module';

@NgModule({
  declarations: [
    JobComponent,
    JobListComponent,
    GradeComponent,
    GradeListComponent,
    RolesReportComponent
  ],
  imports: [
    CommonModule,
    AppComponentsModule,
    JobRoutingModule
  ],
  providers: [
  ]
})
export class JobModule {}
