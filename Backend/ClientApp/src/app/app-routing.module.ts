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
import { PersonResolverService } from './people/person-resolver.service';
import { PeopleResolveService } from './people/list/people-resolve.service';
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
import { PeopleWithLeaveResolverService } from './people/leave-request/people-with-leave-resolver.service';
import { LeaveReportComponent } from './people/leave-request/leave-report/leave-report.component';
import { MissionOrgListComponent } from './mission-org/list/mission-org-list.component';
import { MissionOrgResolverService } from './mission-org/edit/mission-org-resolver.service';
import { MissionOrgListResolverService } from './mission-org/list/mission-org-list-resolver.service';
import { MissionOrgComponent } from './mission-org/edit/mission-org.component';
import { CanDeactivateGuard } from './services/can-deactivate.guard';
import { SandboxComponent } from './components/sandbox/sandbox.component';
import { StaffSummariesResolveService } from './people/staff/staff-report/staff-summaries-resolve.service';
import { PersonRequiredGuard } from './services/person-required.guard';
import { OrgTreeComponent } from './org-tree/org-tree.component';
import { AllRolesResolverService } from './org-tree/all-roles-resolver.service';
import { EvaluationSummaryResolveService } from './people/evaluation-report/evaluation-summary-resolve.service';
import { OrgTreeDataResolverService } from './org-tree/org-tree-data-resolver.service';
import { EndorsementListComponent } from './endorsement/list/endorsement-list.component';
import { EndorsementListResolverService } from './endorsement/endorsement-list-resolver.service';
import { EndorsementComponent } from './endorsement/edit/endorsement.component';
import { EndorsementResolverService } from './endorsement/endorsement-resolver.service';
import { PolicyGuard } from './services/auth/policy.guard';
import { CalendarComponent } from './calendar/calendar.component';
import { HolidayListComponent } from './holiday/list/holiday-list.component';
import { HolidayListResolverService } from './holiday/holiday-list-resolver.service';
import { HolidayResolverService } from './holiday/holiday-resolver.service';
import { HolidayComponent } from './holiday/edit/holiday.component';

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
            canActivate: [PolicyGuard],
            data: {
              requirePolicy: 'userManager'
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
        loadChildren: () => import('./people/people.module').then(m => m.PeopleModule),
      },
      {
        path: 'job',
        loadChildren: () => import('./job/job.module').then(m => m.JobModule)
      },
      {
        path: 'groups',
        canActivate: [PolicyGuard],
        data: {
          requirePolicy: 'orgGroupManager'
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
            },
            data: {
              mine: true
            }
          },
          {
            path: 'report',
            children: [
              {
                path: 'supervisor',
                canActivate: [PolicyGuard],
                data: {
                  requirePolicy: 'leaveSupervisor',
                  supervisor: true
                },
                component: LeaveReportComponent,
                resolve: {
                  people: PeopleWithLeaveResolverService
                }
              },
              {
                path: 'supervisor/:year',
                canActivate: [PolicyGuard],
                data: {
                  requirePolicy: 'leaveSupervisor',
                  supervisor: true
                },
                component: LeaveReportComponent,
                resolve: {
                  people: PeopleWithLeaveResolverService
                }
              },
              {
                path: '',
                canActivate: [PolicyGuard],
                data: {
                  requirePolicy: 'leaveManager'
                },
                children: [
                  {
                    path: 'all/:year',
                    component: LeaveReportComponent,
                    resolve: {
                      people: PeopleWithLeaveResolverService,
                    },
                    data: {
                      all: true
                    }
                  },
                  {
                    path: 'all',
                    component: LeaveReportComponent,
                    resolve: {
                      people: PeopleWithLeaveResolverService,
                    },
                    data: {
                      all: true
                    }
                  }
                ]
              }
            ],
          },
          {
            path: 'list',
            children: [
              {
                path: 'mine',
                canActivate: [PersonRequiredGuard],
                component: LeaveListComponent,
                resolve: {
                  leave: LeaveListResolverService
                },
                data: {
                  mine: true
                }
              },
              {
                path: 'supervisor',
                component: LeaveListComponent,
                resolve: {
                  leave: LeaveListResolverService
                },
                data: {
                  supervisor: true
                }
              },
              {
                path: '',
                canActivate: [PolicyGuard],
                data: {
                  requirePolicy: 'staffLeaveManager'
                },
                children: [
                  {
                    path: 'supervisor/:supervisorId',
                    component: LeaveListComponent,
                    resolve: {
                      leave: LeaveListResolverService
                    }
                  },
                  {
                    path: 'all',
                    component: LeaveListComponent,
                    resolve: {
                      leave: LeaveListResolverService
                    },
                    data: {
                      all: true,
                      requirePolicy: 'leaveManager'
                    }
                  },
                  {
                    path: ':personId',
                    component: LeaveListComponent,
                    resolve: {
                      leave: LeaveListResolverService
                    }
                  }
                ]
              }
            ]
          },
        ]
      },
      {
        path: 'holiday',
        canActivate: [PolicyGuard],
        data: {
          requirePolicy: 'hrEdit'
        },
        children: [
          {
            path: 'list',
            component: HolidayListComponent,
            resolve: {
              holidays: HolidayListResolverService
            }
          },
          {
            path: 'edit/:id',
            component: HolidayComponent,
            canDeactivate: [CanDeactivateGuard],
            resolve: {
              holiday: HolidayResolverService
            }
          }
        ]
      },
      {
        path: 'training',
        canActivate: [PolicyGuard],
        data: {
          requirePolicy: 'hrEdit'
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
        canActivate: [PolicyGuard],
        data: {
          requirePolicy: 'missionOrgManager'
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
        canActivate: [PolicyGuard],
        data: {
          requirePolicy: 'orgChart'
        },
        children: [
          {
            path: 'all',
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
        path: 'endorsement',
        canActivate: [PolicyGuard],
        data: {
          requirePolicy: 'endorsementManager'
        },
        children: [
          {
            path: 'list',
            component: EndorsementListComponent,
            resolve: {
              endorsements: EndorsementListResolverService
            }
          },
          {
            path: 'edit/:id',
            component: EndorsementComponent,
            resolve: {
              endorsement: EndorsementResolverService
            }
          }
        ]
      },
      {
        path: 'calendar',
        children: [
          {
            path: 'supervisor',
            component: CalendarComponent,
            resolve: {
              leave: LeaveListResolverService
            },
            canActivate: [PolicyGuard],
            data: {
              requirePolicy: 'leaveSupervisor',
              supervisor: true
            }
          },
          {
            path: 'mine',
            component: CalendarComponent,
            resolve: {
              leave: LeaveListResolverService
            },
            data: {
              mine: true
            }
          },
          {
            path: 'public',
            component: CalendarComponent,
            resolve: {
              leave: LeaveListResolverService
            },
            data: {
              public: true
            }
          },
          {
            path: '',
            canActivate: [PolicyGuard],
            data: {
              requirePolicy: 'leaveManager'
            },
            children: [
              {
                path: 'supervisor/:supervisorId',
                component: CalendarComponent,
                resolve: {
                  leave: LeaveListResolverService
                }
              },
              {
                path: 'all',
                component: CalendarComponent,
                resolve: {
                  leave: LeaveListResolverService
                },
                data: {
                  all: true
                }
              },
              {
                path: ':personId',
                component: CalendarComponent,
                resolve: {
                  leave: LeaveListResolverService
                }
              }
            ]
          }
        ]
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
    MissionOrgResolverService,
    MissionOrgListResolverService,
    CanDeactivateGuard,
    StaffSummariesResolveService,
    PersonRequiredGuard,
    AllRolesResolverService,
    EvaluationSummaryResolveService,
    OrgTreeDataResolverService,
    EndorsementListResolverService,
    EndorsementResolverService,
    HolidayListResolverService,
    HolidayResolverService
  ]
})
export class AppRoutingModule {
}
