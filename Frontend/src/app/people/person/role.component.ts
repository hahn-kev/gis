import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Role } from '../role';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Job } from '../../job/job';
import { JobService } from '../../job/job.service';
import { LazyLoadService } from '../../services/lazy-load.service';
import { Observable } from 'rxjs/Observable';

@Component({
  selector: 'app-role',
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.scss']
})
export class RoleComponent implements OnInit {
  @Input() readonly = false;
  @Input() hideJob = false;
  @Input() role: Role;
  @Input() formId: string;
  @ViewChild('form') form: NgForm;
  jobs: Observable<Job[]>;

  constructor(private route: ActivatedRoute, lazyLoadService: LazyLoadService, jobService: JobService) {
    this.jobs = lazyLoadService.share('jobs', () => jobService.list());
  }

  ngOnInit() {
  }

}
