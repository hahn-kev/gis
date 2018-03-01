import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Person, PersonWithOthers, Staff } from '../person';
import { PersonService } from '../person.service';
import { Role } from '../role';
import { OrgGroup } from '../groups/org-group';
import { MatDialog } from '@angular/material';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { NgModel } from '@angular/forms';
import { EmergencyContactExtended } from '../emergency-contact';
import { EmergencyContactComponent } from './emergency-contact/emergency-contact.component';
import { RoleComponent } from './role.component';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html',
  styleUrls: ['./person.component.scss']
})
export class PersonComponent implements OnInit {
  public isNew: boolean;
  public person: PersonWithOthers;
  public groups: OrgGroup[];
  public people: Person[];
  public peopleMap: { [key: string]: Person } = {};
  public newEmergencyContact = new EmergencyContactExtended();
  public newRole = new Role();
  @ViewChild('newEmergencyContactEl') newEmergencyContactEl: EmergencyContactComponent;
  @ViewChild('newRoleEl') newRoleEl: RoleComponent;
  @ViewChild('isStaff') isStaffElement: NgModel;

  constructor(private route: ActivatedRoute,
              private personService: PersonService,
              private router: Router,
              private dialog: MatDialog) {
    this.route.data.subscribe((value: {
      person: PersonWithOthers,
      groups: OrgGroup[],
      people: Person[]
    }) => {
      this.person = value.person;
      this.groups = value.groups;
      this.isNew = !this.person.id;
      this.newRole.personId = this.person.id;
      this.newEmergencyContact.personId = this.person.id;
      this.people = value.people.filter(person => person.id != value.person.id);
      this.peopleMap = this.people.reduce((map, currentValue) => {
        map[currentValue.id] = currentValue;
        return map;
      }, {});
      this.newEmergencyContact.order = this.person.emergencyContacts.length + 1;
    });
  }

  ngOnInit(): void {

  }

  async isStaffChanged(isStaff: boolean): Promise<void> {
    if (isStaff) {
      this.person.staff = new Staff();
      return;
    }
    //deleting?
    if (!this.isNew) {
      const dialogRef = this.dialog.open(ConfirmDialogComponent,
        {
          data: ConfirmDialogComponent.Options(`Deleting staff, data will be lost, this can not be undone`,
            'Delete',
            'Cancel')
        });
      let result = await dialogRef.afterClosed().toPromise();
      if (!result) {
        //roll back switch
        this.isStaffElement.control.setValue(true, {emitEvent: false});
        return;
      }
    }
    this.person.staff = null;
  }

  async save(): Promise<void> {
    let savedPerson = await this.personService.updatePerson(this.person);
    if (this.isNew) {
      this.router.navigate(['people', 'edit', savedPerson.id]);
    } else {
      this.router.navigate(['/people/list']);
    }
  }

  async deletePerson() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog, `Delete person?`, 'Delete', 'Cancel');
    if (!result) return;
    await this.personService.deletePerson(this.person.id);
    this.router.navigate(['/people/list']);
  }

  async saveRole(role: Role, isNew = false): Promise<void> {
    role = await this.personService.updateRole(role);
    if (isNew) {
      this.person.roles = [...this.person.roles, role];
      this.newRole = new Role();
      this.newRole.personId = this.person.id;
      this.newRoleEl.form.resetForm();
    }
  }

  async deleteRole(role: Role) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent,
      {
        data: ConfirmDialogComponent.Options(`Deleting role: "${role.name}", data will be lost, this can not be undone`,
          'Delete',
          'Cancel')
      });
    let result = await dialogRef.afterClosed().toPromise();
    if (result) {
      await this.personService.deleteRole(role.id);
      this.person.roles = this.person.roles.filter(value => value.id != role.id);
    }
  }

  async saveEmergencyContact(emergencyContact: EmergencyContactExtended, isNew = false) {
    emergencyContact = await this.personService.updateEmergencyContact(emergencyContact);
    if (isNew) {
      this.person.emergencyContacts = [...this.person.emergencyContacts, emergencyContact];
      this.newEmergencyContact = new EmergencyContactExtended();
      this.newEmergencyContact.personId = this.person.id;
      this.newEmergencyContact.order = this.person.emergencyContacts.length + 1;
      this.newEmergencyContactEl.form.resetForm();
    }
  }

  async deleteEmergencyContact(emergencyContact: EmergencyContactExtended) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent,
      {
        data: ConfirmDialogComponent.Options(`Deleting Emergency Contact: "${emergencyContact.contactPreferedName}"?`,
          'Delete',
          'Cancel')
      });
    let result = await dialogRef.afterClosed().toPromise();
    if (result) {
      await this.personService.deleteEmergencyContact(emergencyContact.id);
      this.person.emergencyContacts = this.person.emergencyContacts.filter(value => value.id != emergencyContact.id);
    }
  }
}
