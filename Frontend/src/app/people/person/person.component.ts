import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Person, PersonWithOthers, Staff } from '../person';
import { PersonService } from '../person.service';
import { Role } from '../role';
import { OrgGroup } from '../groups/org-group';
import { MatDialog } from '@angular/material';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { NgModel } from '@angular/forms';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html',
  styleUrls: ['./person.component.scss']
})
export class PersonComponent implements OnInit {
  public person: PersonWithOthers;
  public groups: OrgGroup[];
  public people: Person[];
  public newRole = new Role();
  @ViewChild('isStaff') isStaffElement: NgModel;

  constructor(private route: ActivatedRoute,
    private personService: PersonService,
    private router: Router,
    private dialog: MatDialog) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value: { person: PersonWithOthers, groups: OrgGroup[], people: Person[] }) => {
      this.person = value.person;
      this.groups = value.groups;
      this.newRole.personId = this.person.id;
      this.people = value.people.filter(person => person.id != value.person.id);
    });
  }

  async isStaffChanged(isStaff: boolean): Promise<void> {
    if (isStaff) {
      this.person.staff = new Staff();
      return;
    }
    //deleting?
    const dialogRef = this.dialog.open(ConfirmDialogComponent,
      {
        data: ConfirmDialogComponent.Options(`Deleting staff, data will be lost, this can not be undone`,
          'Delete',
          'Cancel')
      });
    let result = await dialogRef.afterClosed().toPromise();
    if (result) {
      this.person.staff = null;
    } else {
      //roll back switch
      this.isStaffElement.control.setValue(true, {emitEvent: false});
    }
  }

  async save(): Promise<void> {
    await this.personService.updatePerson(this.person);
    this.router.navigate(['/people/list']);
  }

  async saveRole(role: Role, isNew = false): Promise<void> {
    role = await this.personService.updateRole(role);
    if (isNew) {
      this.person.roles = [...this.person.roles, role];
      this.newRole = new Role();
      this.newRole.personId = this.person.id;
    }
  }
}
