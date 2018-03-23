import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Role } from '../role';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Job } from '../../job/job';

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
  jobs: Job[];

  constructor(private route: ActivatedRoute) {
    this.route.data.subscribe(value => {
      this.jobs = value.jobs;
    })
  }

  ngOnInit() {
  }

}
