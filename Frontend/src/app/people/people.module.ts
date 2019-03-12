import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PersonComponent } from './person/person.component';
import { PeopleListComponent } from './list/people-list.component';
import { EvaluationReportComponent } from './evaluation-report/evaluation-report.component';
import { StaffReportComponent } from './staff/staff-report/staff-report.component';
import { ProfilePictureComponent } from './person/profile-picture/profile-picture.component';
import { EducationComponent } from './person/education/education.component';
import { DateAddPipe } from '../services/date-add.pipe';
import { AppComponentsModule } from '../components/app-components.module';
import { EvaluationComponent } from './person/evaluation/evaluation.component';
import { StaffTrainingComponent } from './person/staff-training/staff-training.component';
import { EmergencyContactComponent } from './person/emergency-contact/emergency-contact.component';
import { DonationComponent } from './person/donor/donation.component';
import { DonorComponent } from './person/donor/donor.component';
import { PeopleRoutingModule } from './people-routing.module';
import { SchoolAidListComponent } from './schoolAidList/school-aid-list.component';

@NgModule({
  declarations: [
    PersonComponent,
    PeopleListComponent,
    EvaluationReportComponent,
    StaffReportComponent,
    ProfilePictureComponent,
    EducationComponent,
    DateAddPipe,
    EvaluationComponent,
    StaffTrainingComponent,
    EmergencyContactComponent,
    DonationComponent,
    DonorComponent,
    SchoolAidListComponent
  ],
  imports: [
    CommonModule,
    AppComponentsModule,
    PeopleRoutingModule
  ],
  exports: []
})
export class PeopleModule {}
