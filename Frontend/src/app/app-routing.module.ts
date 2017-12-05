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
        children: [
          {
            path: 'edit/:id',
            component: PersonComponent,
            resolve: {
              person: PersonResolverService
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
            path: '',
            component: PeopleListComponent,
            resolve: {
              people: PeopleResolveService
            }
          }
        ]
      },
      {
        path: 'groups',
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
            path: '',
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
            path: 'new',
            component: LeaveRequestComponent
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
    GroupsResolveService
  ]
})
export class AppRoutingModule {
}
