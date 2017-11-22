import { Component, OnInit } from '@angular/core';
import { User } from './user';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from './user.service';
import { MatDialog } from '@angular/material';
import { ConfirmDialogComponent } from '../dialog/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent implements OnInit {
  public user: User;
  public isNew: boolean;
  public isSelf: boolean;
  public password: string;
  public errorMessage: string;

  constructor(private route: ActivatedRoute,
              private userService: UserService,
              private router: Router,
              private dialog: MatDialog) {
  }

  ngOnInit() {
    this.route.data.subscribe((data: { user: User, isNew: boolean, isSelf: boolean }) => {
      this.user = data.user;
      this.isNew = data.isNew;
      this.isSelf = data.isSelf;
    });
  }


  async saveUser() {
    await this.userService.saveUser(this.user, this.password, this.isNew, this.isSelf);
    this.router.navigate([this.isSelf ? '/home' : '/user/admin']);
  }

  async grantAdmin() {
    await this.userService.grantAdmin(this.user.userName);
    this.user.isAdmin = true;
  }

  async revokeAdmin() {
    await this.userService.revokeAdmin(this.user.userName);
    this.user.isAdmin = false;
  }

  deleteUser() {
    let dialogRef = this.dialog.open(ConfirmDialogComponent, {data: ConfirmDialogComponent.Options(`Delete User ${this.user.userName}?`, 'Delete', 'Cancel')});
    dialogRef.afterClosed().subscribe(async result => {
      if (result) {
        await this.userService.deleteUser(this.user.id);
        this.router.navigate(['/user/admin']);
      }
    });
  }

}
