import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PolicyGuard } from '../services/auth/policy.guard';
import { JobListComponent } from './list/job-list.component';
import { JobFilledListResolverService } from './list/job-filled-list-resolver.service';
import { JobComponent } from './job/job.component';
import { CanDeactivateGuard } from '../services/can-deactivate.guard';
import { JobResolverService } from './job-resolver.service';
import { GroupsResolveService } from '../people/groups/org-group-list/groups-resolve.service';
import { GradeListResolverService } from './grade/grade-list-resolver.service';
import { RolesReportComponent } from '../people/roles-report/roles-report.component';
import { RolesResolverService } from '../people/roles-report/roles-resolver.service';
import { GradeListComponent } from './grade/list/grade-list.component';
import { GradeComponent } from './grade/edit/grade.component';
import { GradeResolverService } from './grade/grade-resolver.service';

const routes: Routes = [
  {
    path: '',
    canActivate: [PolicyGuard],
    data: {
      requirePolicy: 'hrEdit'
    },
    children: [
      {
        path: 'list',
        component: JobListComponent,
        resolve: {
          jobs: JobFilledListResolverService
        }
      },
      {
        path: 'edit/:id',
        component: JobComponent,
        canDeactivate: [CanDeactivateGuard],
        resolve: {
          job: JobResolverService,
          groups: GroupsResolveService,
          grades: GradeListResolverService
        }
      },
      {
        path: 'grade',
        children: [
          {
            path: 'list',
            component: GradeListComponent,
            resolve: {
              grades: GradeListResolverService
            }
          },
          {
            path: 'edit/:id',
            component: GradeComponent,
            canDeactivate: [CanDeactivateGuard],
            resolve: {
              grade: GradeResolverService
            }
          }
        ]
      }
    ]
  },
  {
    path: 'report',
    children: [
      {
        canActivate: [PolicyGuard],
        path: 'roles/supervisor/:year',
        component: RolesReportComponent,
        resolve: {
          roles: RolesResolverService
        },
        data: {
          supervisor: true,
          requirePolicy: 'isSupervisor'
        }
      },
      {
        canActivate: [PolicyGuard],
        path: 'roles/supervisor',
        component: RolesReportComponent,
        resolve: {
          roles: RolesResolverService
        },
        data: {
          supervisor: true,
          requirePolicy: 'isSupervisor'
        }
      },
      {
        canActivate: [PolicyGuard],
        path: 'roles/:year',
        component: RolesReportComponent,
        resolve: {
          roles: RolesResolverService
        },
        data: {
          requirePolicy: 'hrEdit'
        },
      },
      {
        canActivate: [PolicyGuard],
        path: 'roles',
        component: RolesReportComponent,
        resolve: {
          roles: RolesResolverService
        },
        data: {
          requirePolicy: 'hrEdit'
        },
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
  providers: [
    JobResolverService,
    JobFilledListResolverService,
    GradeResolverService,
    GradeListResolverService
  ]
})
export class JobRoutingModule { }
