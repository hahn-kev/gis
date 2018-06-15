import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserComponent } from './user/user.component';
import { LoginComponent } from './login/login.component';
import { AdminComponent } from './user/admin/admin.component';
import { UserResolveService } from './user/user-resolve.service';
import { HomeComponent } from './home/home.component';
import { IsNewResolverService } from './user/is-new-resolver.service';
import { MessageComponent } from './message/message.component';
import { IsSelfResolverService } from './user/is-self-resolver.service';
import { LoginService } from './services/auth/login.service';
import { RoleGuardService } from './services/auth/role-guard.service';
import { PersonComponent } from './people/person/person.component';
import { PersonResolverService } from './people/person-resolver.service';
import { PeopleListComponent } from './people/list/people-list.component';
import { PeopleResolveService } from './people/list/people-resolve.service';
import { RolesReportComponent } from './people/roles-report/roles-report.component';
import { RolesResolverService } from './people/roles-report/roles-resolver.service';
import { GroupComponent } from './people/groups/group/group.component';
import { GroupResolveService } from './people/groups/group/group-resolve.service';
import { OrgGroupListComponent } from './people/groups/org-group-list/org-group-list.component';
import { GroupsResolveService } from './people/groups/org-group-list/groups-resolve.service';
import { LeaveRequestComponent } from './people/leave-request/leave-request.component';
import { LeaveListComponent } from './people/leave-request/leave-list/leave-list.component';
import { LeaveListResolverService } from './people/leave-request/leave-list/leave-list-resolver.service';
import { TrainingListComponent } from './people/training-requirement/training-list/training-list.component';
import { TrainingListResolverService } from './people/training-requirement/training-list-resolver.service';
import { TrainingEditComponent } from './people/training-requirement/training-edit/training-edit.component';
import { TrainingResolverService } from './people/training-requirement/training-resolver.service';
import { TrainingReportComponent } from './people/training-requirement/training-report/training-report.component';
import { StaffTrainingResolverService } from './people/training-requirement/staff-training-resolver.service';
import { StaffResolveService } from './people/staff-resolve.service';
import { LeaveRequestResolverService } from './people/leave-request/leave-request-resolver.service';
import { EmergencyContactResolverService } from './people/emergency-contact-resolver.service';
import { SelfService } from './people/self/self.service';
import { PeopleWithLeaveResolverService } from './people/leave-request/people-with-leave-resolver.service';
import { LeaveReportComponent } from './people/leave-request/leave-report/leave-report.component';
import { StaffReportComponent } from './people/staff/staff-report/staff-report.component';
import { JobResolverService } from './job/job-resolver.service';
import { JobListComponent } from './job/list/job-list.component';
import { JobComponent } from './job/job/job.component';
import { JobListResolverService } from './job/job-list-resolver.service';
import { MissionOrgListComponent } from './mission-org/list/mission-org-list.component';
import { MissionOrgResolverService } from './mission-org/edit/mission-org-resolver.service';
import { MissionOrgListResolverService } from './mission-org/list/mission-org-list-resolver.service';
import { MissionOrgComponent } from './mission-org/edit/mission-org.component';
import { GradeListComponent } from './job/grade/list/grade-list.component';
import { GradeListResolverService } from './job/grade/grade-list-resolver.service';
import { GradeResolverService } from './job/grade/grade-resolver.service';
import { GradeComponent } from 'app/job/grade/edit/grade.component';
import { JobFilledListResolverService } from './job/list/job-filled-list-resolver.service';
import { CanDeactivateGuard } from './services/can-deactivate.guard';
import { SandboxComponent } from './components/sandbox/sandbox.component';
import { SchoolAidResolveService } from './people/list/school-aid-resolve.service';
import { StaffSummariesResolveService } from './people/staff/staff-report/staff-summaries-resolve.service';
import { PersonRequiredGuard } from './services/person-required.guard';
import { OrgTreeComponent } from './org-tree/org-tree.component';
import { AllRolesResolverService } from './org-tree/all-roles-resolver.service';
import { EvaluationReportComponent } from './people/evaluation-report/evaluation-report.component';
import { EvaluationSummaryResolveService } from './people/evaluation-report/evaluation-summary-resolve.service';
import { OrgTreeDataResolverService } from './org-tree/org-tree-data-resolver.service';
import { EndorsementListComponent } from './endorsement/list/endorsement-list.component';

const routes: Routes = [
  {
    path: '',
    canActivate: [LoginService],
    children: [
      {
        path: 'user',
        children: [
          {
            path: 'admin',
            component: AdminComponent,
            canActivate: [RoleGuardService],
            data: {
              requireRole: 'admin'
            }
          },
          {
            path: 'edit/:name',
            component: UserComponent,
            canDeactivate: [CanDeactivateGuard],
            resolve: {
              user: UserResolveService,
              isNew: IsNewResolverService,
              isSelf: IsSelfResolverService
            }
          }
        ]
      },
      {
        path: 'people',
        canActivate: [RoleGuardService],
        data: {
          requireRole: ['admin', 'hr', 'registrar']
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
                component: PeopleListComponent,
                data: {title: 'School Aids'},
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
        canActivate: [RoleGuardService],
        data: {
          requireRole: ['admin', 'hr']
        },
        children: [
          {
            path: 'report',
            component: StaffReportComponent,
            resolve: {
              staff: StaffSummariesResolveService
            }
          }
        ]
      },
      {
        path: 'job',
        canActivate: [RoleGuardService],
        data: {
          requireRole: ['admin', 'hr']
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
            path: 'report',
            children: [
              {
                path: 'roles/:year',
                component: RolesReportComponent,
                resolve: {
                  roles: RolesResolverService
                }
              },
              {
                path: 'roles',
                component: RolesReportComponent,
                resolve: {
                  roles: RolesResolverService
                }
              }
            ]
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
        path: 'groups',
        canActivate: [RoleGuardService],
        data: {
          requireRole: ['admin', 'hradmin']
        },
        children: [
          {
            path: 'edit/:id',
            component: GroupComponent,
            canDeactivate: [CanDeactivateGuard],
            resolve: {
              group: GroupResolveService,
              groups: GroupsResolveService,
              staff: StaffResolveService
            }
          },
          {
            path: 'list',
            component: OrgGroupListComponent,
            resolve: {
              groups: GroupsResolveService
            }
          }
        ]
      },
      {
        path: 'leave-request',
        children: [
          {
            path: 'edit/:id',
            component: LeaveRequestComponent,
            canActivate: [PersonRequiredGuard],
            canDeactivate: [CanDeactivateGuard],
            resolve: {
              leaveRequest: LeaveRequestResolverService,
              people: PeopleWithLeaveResolverService
            }
          },
          {
            path: 'list/mine',
            canActivate: [PersonRequiredGuard],
            component: LeaveListComponent,
            resolve: {
              leave: LeaveListResolverService
            }
          },
          {
            path: 'list/:personId',
            component: LeaveListComponent,
            resolve: {
              leave: LeaveListResolverService
            }
          },
          {
            path: 'list',
            component: LeaveListComponent,
            resolve: {
              leave: LeaveListResolverService
            },
            canActivate: [RoleGuardService],
            data: {
              requireRole: ['admin', 'hr']
            }
          },
          {
            path: 'report',
            component: LeaveReportComponent,
            resolve: {
              people: PeopleWithLeaveResolverService
            }
          }
        ]
      },
      {
        path: 'training',
        canActivate: [RoleGuardService],
        data: {
          requireRole: ['admin', 'hr']
        },
        children: [
          {
            path: 'list',
            component: TrainingListComponent,
            resolve: {
              trainingRequirements: TrainingListResolverService
            }
          },
          {
            path: 'edit/:id',
            component: TrainingEditComponent,
            canDeactivate: [CanDeactivateGuard],
            resolve: {
              training: TrainingResolverService,
              people: PeopleResolveService,
              groups: GroupsResolveService,
            }
          },
          {
            path: 'report/:year',
            component: TrainingReportComponent,
            resolve: {
              staffTraining: StaffTrainingResolverService,
            }
          },
          {
            path: 'report',
            component: TrainingReportComponent,
            resolve: {
              staffTraining: StaffTrainingResolverService,
            }
          }
        ]
      },
      {
        path: 'mission-org',
        canActivate: [RoleGuardService],
        data: {
          requireRole: ['admin', 'hr', 'registrar']
        },
        children: [
          {
            path: 'list',
            component: MissionOrgListComponent,
            resolve: {
              missionOrgs: MissionOrgListResolverService
            }
          },
          {
            path: 'edit/:id',
            component: MissionOrgComponent,
            canDeactivate: [CanDeactivateGuard],
            resolve: {
              missionOrg: MissionOrgResolverService,
              people: PeopleResolveService
            }
          }
        ]
      },
      {
        path: 'org-tree',
        canActivate: [RoleGuardService],
        data: {
          requireRole: ['admin', 'hr']
        },
        children: [
          {
            path: '',
            component: OrgTreeComponent,
            resolve: {
              treeData: OrgTreeDataResolverService
            }
          },
          {
            path: ':rootId',
            component: OrgTreeComponent,
            resolve: {
              treeData: OrgTreeDataResolverService
            }
          }
        ]
      },
      {
        path: 'endorsements',
        canActivate: [RoleGuardService],
        data: {
          requireRole: ['admin', 'hr']
        },
        children: [
          {
            path: 'list',
            component: EndorsementListComponent
          }
        ]
      },
      {
        path: 'self',
        canActivate: [PersonRequiredGuard],
        component: PersonComponent,
        resolve: {
          person: SelfService,
          people: PeopleResolveService
        }
      },
      {
        path: 'sandbox',
        component: SandboxComponent
      },
      {
        path: 'home',
        component: HomeComponent
      },
      {
        path: '',
        redirectTo: '/home',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'message',
    component: MessageComponent
  },
  {
    path: '**',
    redirectTo: '/home',
    pathMatch: 'full'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {enableTracing: false})],
  exports: [RouterModule],
  providers: [
    UserResolveService,
    IsNewResolverService,
    IsSelfResolverService,
    PersonResolverService,
    PeopleResolveService,
    RolesResolverService,
    GroupResolveService,
    GroupsResolveService,
    LeaveListResolverService,
    TrainingListResolverService,
    TrainingResolverService,
    StaffTrainingResolverService,
    StaffResolveService,
    LeaveRequestResolverService,
    EmergencyContactResolverService,
    PeopleWithLeaveResolverService,
    JobResolverService,
    JobListResolverService,
    MissionOrgResolverService,
    MissionOrgListResolverService,
    GradeResolverService,
    GradeListResolverService,
    JobFilledListResolverService,
    CanDeactivateGuard,
    SchoolAidResolveService,
    StaffSummariesResolveService,
    PersonRequiredGuard,
    AllRolesResolverService,
    EvaluationSummaryResolveService,
    OrgTreeDataResolverService
  ]
})
export class AppRoutingModule {
}
