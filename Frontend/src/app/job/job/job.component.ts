import { Component, OnInit } from '@angular/core';
import { JobService } from '../job.service';
import { JobStatus, JobType, JobWithRoles } from '../job';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatSnackBar } from '@angular/material';
import { OrgGroup } from '../../people/groups/org-group';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { Role, RoleExtended } from '../../people/role';
import { PersonService } from '../../people/person.service';
import { Grade } from '../grade/grade';
import { BaseEditComponent } from '../../components/base-edit-component';
import { LazyLoadService } from '../../services/lazy-load.service';
import { Location } from '@angular/common';
import { Endorsement, RequiredEndorsement, RequiredEndorsementWithName } from '../../endorsement/endorsement';
import { Observable } from 'rxjs';
import { EndorsementService } from '../../endorsement/endorsement.service';
import { JobStatusNamePipe } from '../job-status-name.pipe';
import { JobTypeNamePipe } from '../job-type-name.pipe';

@Component({
  selector: 'app-job',
  templateUrl: './job.component.html',
  styleUrls: ['./job.component.scss'],
  providers: [LazyLoadService, JobStatusNamePipe, JobTypeNamePipe]
})
export class JobComponent extends BaseEditComponent implements OnInit {
  public jobTypes = Object.keys(JobType);

  public jobStatus = Object.keys(JobStatus);

  public job: JobWithRoles;
  public groups: OrgGroup[];
  public grades: Grade[];
  public isNew = false;
  public endorsements: Observable<Endorsement[]>;

  constructor(private jobService: JobService,
              private personService: PersonService,
              private route: ActivatedRoute,
              private router: Router,
              private snackBar: MatSnackBar,
              private location: Location,
              lazyLoad: LazyLoadService,
              private endorsementService: EndorsementService,
              dialog: MatDialog) {
    super(dialog);
    this.endorsements = lazyLoad.share('endorsements', () => this.endorsementService.list());
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
    if (this.job.roles.length > 0) {
      await ConfirmDialogComponent.OpenWait(this.dialog,
        'To delete this job you must first delete all the Roles below, or you could make the job as not current',
        'Close');
      return;
    }

    let result = await ConfirmDialogComponent.OpenWait(this.dialog, 'Delete Job?', 'Delete', 'Cancel');
    if (!result) return;
    await this.jobService.delete(this.job.id).toPromise();
    this.location.back();
    this.snackBar.open(`${this.job.title} Deleted`, null, {duration: 2000});
  }

  async saveRole(role: Role): Promise<void> {
    await this.personService.updateRole(role);
    this.snackBar.open(`Role Saved`, null, {duration: 2000});
  }

  async deleteRole(role: RoleExtended) {

    let result = await ConfirmDialogComponent.OpenWait(
      this.dialog,
      `Delete role for ${role.preferredName} ${role.lastName}?`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.personService.deleteRole(role.id);
    this.job.roles = this.job.roles.filter(value => value.id != role.id);
    this.snackBar.open(`Role Deleted`, null, {duration: 2000});
  }

  createNewRequiredEndorsement = () => new RequiredEndorsementWithName(this.job.id);
  saveRequiredEndorsement = async (item: RequiredEndorsementWithName) => {
    let result = await this.endorsementService.saveRequiredEndorsement(item);
    return {...item, ...result};
  };

  deleteRequiredEndorsement = (item: RequiredEndorsement) => {
    return this.endorsementService.deleteRequiredEndorsement(item.id);
  }
}
