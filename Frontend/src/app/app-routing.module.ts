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
import { SelfComponent } from './people/self/self.component';
import { SelfService } from './people/self/self.service';

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
          requireRole: ['admin', 'hr']
        },
        children: [
          {
            path: 'edit/:id',
            component: PersonComponent,
            resolve: {
              person: PersonResolverService,
              groups: GroupsResolveService,
              people: PeopleResolveService
            }
          },
          {
            path: 'report',
            children: [
              {
                path: 'roles/:start',
                component: RolesReportComponent,
                runGuardsAndResolvers: 'paramsOrQueryParamsChange',
                resolve: {
                  roles: RolesResolverService
                }
              },
              {
                path: 'roles',
                redirectTo: 'roles/during'
              }
            ]
          },
          {
            path: 'list',
            component: PeopleListComponent,
            resolve: {
              people: PeopleResolveService
            }
          }
        ]
      },
      {
        path: 'groups',
        canActivate: [RoleGuardService],
        data: {
          requireRole: ['admin', 'hr']
        },
        children: [
          {
            path: 'edit/:id',
            component: GroupComponent,
            resolve: {
              group: GroupResolveService,
              groups: GroupsResolveService,
              people: PeopleResolveService
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
            resolve: {
              leaveRequest: LeaveRequestResolverService
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
            resolve: {
              training: TrainingResolverService
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
        path: 'self',
        children: [
          {
            path: ':id', component: SelfComponent,
            resolve: {
              self: SelfService
            }
          },
          {
            path: '',
            component: SelfComponent,
            resolve: {
              self: SelfService
            }
          }
        ]
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
  imports: [RouterModule.forRoot(routes)],
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
    EmergencyContactResolverService
  ]
})
export class AppRoutingModule {
}
