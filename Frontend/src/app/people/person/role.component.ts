import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { RoleWithJob } from '../role';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Job } from '../../job/job';
import { JobService } from '../../job/job.service';
import { LazyLoadService } from '../../services/lazy-load.service';
import { take } from 'rxjs/operators';

@Component({
  selector: 'app-role',
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.scss']
})
export class RoleComponent implements OnInit {
  @Input() readonly = false;
  @Input() hideJob = false;
  @Input() role: RoleWithJob;
  @Input() formId: string;
  @ViewChild('form') form: NgForm;
  jobs: Job[] = [];

  constructor(private route: ActivatedRoute, lazyLoadService: LazyLoadService, jobService: JobService) {
    lazyLoadService.share('jobs', () => jobService.list())
      .pipe(take(1))
      .subscribe(value => this.jobs = value);
  }

  ngOnInit() {
  }

  jobChanged(jobId: string) {
    this.role.job = this.jobs.find(value => value.id == jobId);
  }

}
