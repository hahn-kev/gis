import {Component, Input, OnInit, ViewChild} from '@angular/core';
import {RoleWithJob} from '../role';
import {NgForm} from '@angular/forms';
import {ActivatedRoute} from '@angular/router';
import {JobWithOrgGroup} from '../../job/job';
import {JobService} from '../../job/job.service';
import {LazyLoadService} from '../../services/lazy-load.service';
import {tap} from 'rxjs/operators';
import {Observable} from 'rxjs';

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
  jobs: JobWithOrgGroup[] = [];
  jobsObservable: Observable<JobWithOrgGroup[]>;

  constructor(private route: ActivatedRoute, lazyLoadService: LazyLoadService, jobService: JobService) {
    this.jobsObservable = lazyLoadService
      .share('jobs', () => jobService.list())
      .pipe(tap(x => this.jobs = x));
  }

  ngOnInit() {
  }

  jobChanged(jobId: string) {
    this.role.job = this.jobs.find(value => value.id == jobId);
  }

}
