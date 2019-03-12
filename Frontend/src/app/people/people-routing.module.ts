import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PolicyGuard } from '../services/auth/policy.guard';
import { PersonComponent } from './person/person.component';
import { CanDeactivateGuard } from '../services/can-deactivate.guard';
import { PersonResolverService } from './person-resolver.service';
import { PeopleResolveService } from './list/people-resolve.service';
import { PeopleListComponent } from './list/people-list.component';
import { SchoolAidResolveService } from './schoolAidList/school-aid-resolve.service';
import { EvaluationReportComponent } from './evaluation-report/evaluation-report.component';
import { EvaluationSummaryResolveService } from './evaluation-report/evaluation-summary-resolve.service';
import { StaffReportComponent } from './staff/staff-report/staff-report.component';
import { StaffSummariesResolveService } from './staff/staff-report/staff-summaries-resolve.service';
import { PersonRequiredGuard } from '../services/person-required.guard';
import { SelfService } from './self/self.service';
import { SchoolAidListComponent } from './schoolAidList/school-aid-list.component';

const routes: Routes = [
  {
    path: '',
    canActivate: [PolicyGuard],
    data: {
      requirePolicy: 'peopleEdit'
    },
    children: [
      {
        path: 'edit/:id',
        component: PersonComponent,
        canDeactivate: [CanDeactivateGuard],
        resolve: {
          person: PersonResolverService,
          people: PeopleResolveService
        }
      },
      {
        path: 'list',
        component: PeopleListComponent,
        resolve: {
          people: PeopleResolveService
        }
      },
      {
        path: 'school-aid',
        children: [
          {
            path: 'list',
            component: SchoolAidListComponent,
            data: { title: 'School Aids' },
            resolve: {
              people: SchoolAidResolveService
            }
          }
        ]
      },
      {
        path: 'report',
        children: [
          {
            path: 'evaluations',
            component: EvaluationReportComponent,
            resolve: {
              evaluationSummary: EvaluationSummaryResolveService
            }
          }
        ]
      }
    ]
  },
  {
    path: 'staff',
    children: [
      {
        canActivate: [PolicyGuard],
        path: 'report/supervisor',
        component: StaffReportComponent,
        resolve: {
          staff: StaffSummariesResolveService
        },
        data: {
          requirePolicy: 'isSupervisor',
          supervisor: true
        }
      },
      {
        canActivate: [PolicyGuard],
        path: 'report',
        component: StaffReportComponent,
        resolve: {
          staff: StaffSummariesResolveService
        },
        data: {
          requirePolicy: 'hrEdit'
        },
      }
    ]
  },
  {
    path: 'staff/self',
    canActivate: [PersonRequiredGuard],
    component: PersonComponent,
    resolve: {
      person: SelfService,
      people: PeopleResolveService
    }
  },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes)
  ],
  exports: [
    RouterModule
  ],
  providers: [
    PersonResolverService,
    PeopleResolveService,
    SchoolAidResolveService,
    EvaluationSummaryResolveService,
    StaffSummariesResolveService,
    SelfService,
    PersonRequiredGuard
  ]
})
export class PeopleRoutingModule { }
