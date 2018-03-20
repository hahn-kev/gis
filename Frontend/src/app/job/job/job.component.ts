import { Component, OnInit } from '@angular/core';
import { JobService } from '../job.service';
import { Job } from '../job';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatSnackBar } from '@angular/material';
import { OrgGroup } from '../../people/groups/org-group';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-job',
  templateUrl: './job.component.html',
  styleUrls: ['./job.component.scss']
})
export class JobComponent implements OnInit {
  public job: Job;
  public groups: OrgGroup[];
  private isNew = false;

  constructor(private jobService: JobService,
              private route: ActivatedRoute,
              private router: Router,
              private snackBar: MatSnackBar,
              private dialog: MatDialog) {
  }

  ngOnInit() {
    this.route.data.subscribe((value) => {
      this.job = value.job;
      this.groups = value.groups;
      this.isNew = !this.job.id;
    });
  }

  async save() {
    await this.jobService.save(this.job);
    this.router.navigate(['/job/list']);
    this.snackBar.open(`${this.job.title} ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async deleteJob() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog, 'Delete Job?', 'Delete', 'Cancel');
    if (!result) return;
    await this.jobService.delete(this.job.id).toPromise();
    this.router.navigate(['/job/list']);
    this.snackBar.open(`${this.job.title} Deleted`, null, {duration: 2000});

  }
}
