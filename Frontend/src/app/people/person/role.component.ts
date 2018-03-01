import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Role } from '../role';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-role',
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.scss']
})
export class RoleComponent implements OnInit {
  @Input() role: Role;
  @Input() formId: string;
  @ViewChild('form') form: NgForm;

  constructor() {
  }

  ngOnInit() {
  }

}
