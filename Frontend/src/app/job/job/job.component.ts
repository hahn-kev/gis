import { Component, OnInit, ViewChild } from '@angular/core';
import { JobService } from '../job.service';
import { Job, JobType, jobTypeName, JobWithRoles } from '../job';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatSnackBar } from '@angular/material';
import { OrgGroup } from '../../people/groups/org-group';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { Role, RoleExtended, RoleWithJob } from '../../people/role';
import { PersonService } from '../../people/person.service';
import { Grade } from '../grade/grade';
import { BaseEditComponent } from '../../components/base-edit-component';
import { LazyLoadService } from '../../services/lazy-load.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-job',
  templateUrl: './job.component.html',
  styleUrls: ['./job.component.scss'],
  providers: [LazyLoadService]
})
export class JobComponent extends BaseEditComponent implements OnInit {
  public jobTypes = Object.keys(JobType);
  public typeName = jobTypeName;
  public job: JobWithRoles;
  public groups: OrgGroup[];
  public grades: Grade[];
  public isNew = false;

  constructor(private jobService: JobService,
              private personService: PersonService,
              private route: ActivatedRoute,
              private router: Router,
              private snackBar: MatSnackBar,
              private location: Location,
              dialog: MatDialog) {
    super(dialog);
  }

  ngOnInit() {
    this.route.data.subscribe((value) => {
      this.job = value.job;
      this.groups = value.groups;
      this.grades = value.grades;
      this.isNew = !this.job.id;
    });
  }

  async save() {
    await this.jobService.save(this.job);
    this.location.back();
    this.snackBar.open(`${this.job.title} ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async deleteJob() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog, 'Delete Job?', 'Delete', 'Cancel');
    if (!result) return;
    await this.jobService.delete(this.job.id).toPromise();
    this.router.navigate(['/job/list']);
    this.snackBar.open(`${this.job.title} Deleted`, null, {duration: 2000});
  }

  async saveRole(role: Role): Promise<void> {
    await this.personService.updateRole(role);
    this.snackBar.open(`Role Saved`, null, {duration: 2000});
  }

  async deleteRole(role: RoleExtended) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent,
      {
        data: ConfirmDialogComponent.Options(`Delete role for ${role.preferredName} ${role.lastName}?`,
          'Delete',
          'Cancel')
      });
    let result = await dialogRef.afterClosed().toPromise();
    if (!result) return;
    await this.personService.deleteRole(role.id);
    this.job.roles = this.job.roles.filter(value => value.id != role.id);
    this.snackBar.open(`Role Deleted`, null, {duration: 2000});
  }
}
