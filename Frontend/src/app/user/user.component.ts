import { Component, OnInit } from '@angular/core';
import { User } from './user';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from './user.service';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ConfirmDialogComponent } from '../dialog/confirm-dialog/confirm-dialog.component';
import { PersonService } from '../people/person.service';
import { Observable } from 'rxjs/Observable';
import { Person } from '../people/person';
import { environment } from '../../environments/environment';
import { LoginService } from '../services/auth/login.service';
import { AuthenticateService } from '../services/auth/authenticate.service';
import { BaseEditComponent } from '../components/base-edit-component';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent extends BaseEditComponent implements OnInit {
  public isDev = !environment.production;
  public user: User;
  public isNew: boolean;
  public isSelf: boolean;
  public password: string;
  public errorMessage: string;
  public people: Observable<Person[]>;
  public roles = [
    {
      title: 'Admin',
      name: 'admin',
      update: (user: User, value: boolean) => user.isAdmin = value,
      value: (user: User) => user.isAdmin,
      show: (user: User) => true
    },
    {
      title: 'HR',
      name: 'hr',
      update: (user: User, value: boolean) => user.isHr = value,
      value: (user: User) => user.isHr,
      show: (user: User) => !user.isHrAdmin
    },
    {
      title: 'HR Admin',
      name: 'hr,hradmin',
      update: (user: User, value: boolean) => user.isHrAdmin = user.isHr = value,
      value: (user: User) => user.isHrAdmin,
      show: (user: User) => true
    }];

  constructor(private route: ActivatedRoute,
              private userService: UserService,
              private authService: AuthenticateService,
              private router: Router,
              dialog: MatDialog,
              private personService: PersonService,
              private snackBar: MatSnackBar) {
    super(dialog);
    this.people = this.personService.getAll();
  }

  ngOnInit(): void {
    this.route.data.subscribe((data: { user: User, isNew: boolean, isSelf: boolean }) => {
      this.user = data.user;
      this.isNew = data.isNew;
      this.isSelf = data.isSelf;
    });
  }


  async saveUser(): Promise<void> {
    await this.userService.saveUser(this.user, this.password, this.isNew, this.isSelf);
    this.router.navigate([this.isSelf ? '/home' : '/user/admin']);
  }

  async grantRole(role: { name: string, update: (user: User, value: boolean) => void }): Promise<void> {
    await this.userService.grantRole(role.name, this.user.id);
    role.update(this.user, true);
    this.snackBar.open(`Role granted, user must logout and login to use new role`, null, {duration: 2000});
  }

  async revokeRole(role: { name: string, update: (user: User, value: boolean) => void }): Promise<void> {
    await this.userService.revokeRole(role.name, this.user.id);
    role.update(this.user, false);
    this.snackBar.open(`Role revoked, user must logout and login to lose the role`, null, {duration: 2000});
  }

  async deleteUser() {
    let result = await ConfirmDialogComponent.OpenWait(
      this.dialog,
      `Delete User ${this.user.userName}?`,
      'Delete',
      'Cancel');

    if (result) {
      await this.userService.deleteUser(this.user.id);
      this.router.navigate(['/user/admin']);
    }

  }

  async impersonate() {
    await this.authService.impersonate(this.user.email);
    this.snackBar.open(
      `You're now logged in as ${this.user.userName} just logout to stop impersonating them`,
      null,
      {duration: 2000});
    this.router.navigate(['/home']);
  }

}
