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
            path: '',
            component: PeopleListComponent,
            resolve: {
              people: PeopleResolveService
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
    PeopleResolveService
  ]
})
export class AppRoutingModule {
}
