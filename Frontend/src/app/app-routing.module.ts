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

const routes: Routes = [
  {
    path: '',
    canActivate: [LoginService],
    children: [
      {
        path: 'your-rights',
        redirectTo: 'cms/your-rights'
      },
      {
        path: 'life-lessons',
        redirectTo: 'cms/life-lessons'
      },
      {
        path: 'user/admin',
        component: AdminComponent,
        canActivate: [RoleGuardService],
        data: {
          requireRole: 'admin'
        }
      },
      {
        path: 'user/edit/:name',
        component: UserComponent,
        resolve: {
          user: UserResolveService,
          isNew: IsNewResolverService,
          isSelf: IsSelfResolverService
        }
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
  providers: [UserResolveService, IsNewResolverService, IsSelfResolverService]
})
export class AppRoutingModule {
}
