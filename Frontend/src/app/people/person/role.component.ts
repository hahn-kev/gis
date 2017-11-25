import { Component, Input, OnInit } from '@angular/core';
import { Role } from '../role';

@Component({
  selector: 'app-role',
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.scss']
})
export class RoleComponent implements OnInit {
  @Input() role: Role;

  constructor() {
  }

  ngOnInit() {
  }

}
