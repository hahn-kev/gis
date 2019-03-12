import { Component, OnInit } from '@angular/core';
import { User } from './user';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from './user.service';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ConfirmDialogComponent } from '../dialog/confirm-dialog/confirm-dialog.component';
import { PersonService } from '../people/person.service';
import { Observable } from 'rxjs';
import { Person } from '../people/person';
import { environment } from '../../environments/environment';
import { AuthenticateService } from '../services/auth/authenticate.service';
import { BaseEditComponent } from '../components/base-edit-component';
import { Location } from '@angular/common';

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

  constructor(private route: ActivatedRoute,
              private userService: UserService,
              private authService: AuthenticateService,
              private router: Router,
              private location: Location,
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

  async updateRoles(roles: string[]) {
    if (roles.includes('hradmin') && !roles.includes('hr')) roles.push('hr');
    for (const newRole of roles) {
      if (this.user.roles.includes(newRole)) continue;
      await this.grantRole(newRole);
    }
    const existingRoles = this.user.roles;
    for (const oldRole of existingRoles) {
      if (roles.includes(oldRole)) continue;
      await this.revokeRole(oldRole);
    }
  }

  async grantRole(role: string): Promise<void> {
    await this.userService.grantRole(role, this.user.id);
    this.user.roles = [...this.user.roles, role];
    this.snackBar.open(`Role granted, user must logout and login to use new role`, null, {duration: 2000});
  }

  async revokeRole(role: string): Promise<void> {
    await this.userService.revokeRole(role, this.user.id);
    this.user.roles = this.user.roles.filter(value => value != role);
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
    await this.router.navigate(['/home']);
    this.location.go(this.location.path());
  }

}
