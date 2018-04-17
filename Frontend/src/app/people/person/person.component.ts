import { Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Nationality, NationalityName, Person, PersonWithOthers } from '../person';
import { PersonService } from '../person.service';
import { Role, RoleWithJob } from '../role';
import { OrgGroup } from '../groups/org-group';
import { MatDialog, MatExpansionPanel, MatSnackBar } from '@angular/material';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { NgForm, NgModel } from '@angular/forms';
import { EmergencyContactExtended } from '../emergency-contact';
import { EmergencyContactComponent } from './emergency-contact/emergency-contact.component';
import { RoleComponent } from './role.component';
import { countries } from '../countries';
import { Observable } from 'rxjs/Observable';
import { map } from 'rxjs/operators';
import { endorsments } from '../teacher-endorsements';
import { Job } from '../../job/job';
import { MissionOrg } from '../../mission-org/mission-org';
import { GroupService } from '../groups/group.service';
import { OrgChain } from '../groups/org-chain';
import { CanComponentDeactivate } from '../../services/can-deactivate.guard';
import { StaffWithOrgName } from '../staff';
import { LazyLoadService } from '../../services/lazy-load.service';
import { MissionOrgService } from '../../mission-org/mission-org.service';
import { LeaveType, LeaveTypeName } from '../self/self';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html',
  styleUrls: ['./person.component.scss'],
  providers: [LazyLoadService]
})
export class PersonComponent implements OnInit, CanComponentDeactivate {
  public nationalities = Object.keys(Nationality);
  public nationalityName = NationalityName;
  public leaveTypeName = LeaveTypeName;
  public isNew: boolean;
  public isSelf: boolean;
  public filteredCountries: Observable<string[]>;
  public person: PersonWithOthers;
  public groups: Observable<OrgGroup[]>;
  public missionOrgs: Observable<MissionOrg[]>;
  public people: Person[];
  public jobs: { [key: string]: Job };
  public peopleMap: { [key: string]: Person } = {};
  public newEmergencyContact = new EmergencyContactExtended();
  public newRole = new Role();
  public endorsmentsList = endorsments;
  public staffEndorsments: Array<string> = [];
  @ViewChildren(NgForm) forms: QueryList<NgForm>;
  @ViewChild('newEmergencyContactEl') newEmergencyContactEl: EmergencyContactComponent;
  @ViewChild('newRoleEl') newRoleEl: RoleComponent;
  @ViewChild('isStaff') isStaffElement: NgModel;
  @ViewChild('countriesControl') countriesControl: NgModel;

  constructor(private route: ActivatedRoute,
              private personService: PersonService,
              private groupService: GroupService,
              missionOrgService: MissionOrgService,
              private router: Router,
              private dialog: MatDialog,
              private snackBar: MatSnackBar,
              private lazyLoadService: LazyLoadService) {
    this.isSelf = this.router.url.indexOf('self') != -1;
    this.groups = this.lazyLoadService.share('orgGroups', () => this.groupService.getAll());
    this.missionOrgs = this.lazyLoadService.share('missionOrgs', () => missionOrgService.list());
    this.route.data.subscribe((value: {
      person: PersonWithOthers,
      people: Person[]
    }) => {
      this.person = value.person;
      this.person.leaveDetails.leaveUseages = [
        ...this.person.leaveDetails.leaveUseages
          .filter(l => l.leaveType != LeaveType.Other)
          .sort((a, b) => a.leaveType.localeCompare(b.leaveType)),
        this.person.leaveDetails.leaveUseages.find(l => l.leaveType == LeaveType.Other)];
      if (value.person.staff) {
        this.staffEndorsments = (value.person.staff.endorsements || '').split(',');
      }
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
    this.filteredCountries = this.countriesControl.valueChanges
      .pipe(map(value => countries.filter(country => this.startsWith(country, value || '')))
      );
  }

  startsWith(value: string, test: string) {
    return value.toLowerCase().indexOf(test.toLowerCase()) === 0;
  }

  orgChain(groups: OrgGroup[] | null) {
    if (this.person.staff == null || groups == null) return new OrgChain([]);
    let orgGroup = groups.find(value => value.id == this.person.staff.orgGroupId);
    return this.groupService.buildOrgChain(orgGroup, this.people.concat([this.person]), groups);
  }

  async isStaffChanged(isStaff: boolean): Promise<void> {
    if (isStaff) {
      this.person.staff = new StaffWithOrgName();
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
    let savedPerson = await this.personService.updatePerson(this.person, this.isSelf);
    let commands: Array<any> = ['/people/list'];
    if (this.isNew) {
      commands = ['people', 'edit', savedPerson.id];
    } else if (this.isSelf) {
      commands = ['home'];
    }
    this.router.navigate(commands);
    this.snackBar.open(`${savedPerson.preferredName} Saved`, null, {duration: 2000});
  }

  async deletePerson() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog, `Delete person?`, 'Delete', 'Cancel');
    if (!result) return;
    await this.personService.deletePerson(this.person.id);
    this.router.navigate(['/people/list']);
    this.snackBar.open(`${this.person.preferredName} Deleted`, null, {duration: 2000});
  }

  async saveRole(role: Role, panel: MatExpansionPanel, isNew = false): Promise<void> {
    role = await this.personService.updateRole(role);
    if (isNew) {
      this.person.roles = [...this.person.roles, <RoleWithJob> role];
      if (role.active && this.person.staff) {
        //back end will have done this already, just updating the front end
        this.person.staff.orgGroupId = this.jobs[role.jobId].orgGroupId;
      }
      this.newRole = new Role();
      this.newRole.personId = this.person.id;
      this.newRoleEl.form.resetForm();
      this.snackBar.open(`Role Added`, null, {duration: 2000});
    } else {
      this.snackBar.open(`Role Saved`, null, {duration: 2000});
    }
    panel.close();
  }

  async deleteRole(role: Role) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent,
      {
        data: ConfirmDialogComponent.Options(`Delete role ${this.jobs[role.jobId].title}?`,
          'Delete',
          'Cancel')
      });
    let result = await dialogRef.afterClosed().toPromise();
    if (!result) return;
    await this.personService.deleteRole(role.id);
    this.person.roles = this.person.roles.filter(value => value.id != role.id);
    this.snackBar.open(`Role Deleted`, null, {duration: 2000});

  }

  async saveEmergencyContact(emergencyContact: EmergencyContactExtended, isNew = false) {
    emergencyContact = await this.personService.updateEmergencyContact(emergencyContact);
    if (isNew) {
      this.person.emergencyContacts = [...this.person.emergencyContacts, emergencyContact];
      this.newEmergencyContact = new EmergencyContactExtended();
      this.newEmergencyContact.personId = this.person.id;
      this.newEmergencyContact.order = this.person.emergencyContacts.length + 1;
      this.newEmergencyContactEl.form.resetForm();
      this.snackBar.open(`Emergency Contact Added`, null, {duration: 2000});
    } else {
      this.snackBar.open(`Emergency Contact Saved`, null, {duration: 2000});
    }
  }

  async deleteEmergencyContact(emergencyContact: EmergencyContactExtended) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent,
      {
        data: ConfirmDialogComponent.Options(`Delete Emergency Contact ${emergencyContact.contactPreferedName}?`,
          'Delete',
          'Cancel')
      });
    let result = await dialogRef.afterClosed().toPromise();
    if (!result) return;
    await this.personService.deleteEmergencyContact(emergencyContact.id);
    this.person.emergencyContacts = this.person.emergencyContacts.filter(value => value.id != emergencyContact.id);
    this.snackBar.open(`Emergency Contact Deleted`, null, {duration: 2000});
  }

  canDeactivate() {
    if (!this.forms.some(value => !value.pristine && !value.submitted)) return true;
    return ConfirmDialogComponent.OpenWait(this.dialog, 'Discard Changes?', 'Discard', 'Cancel');
  }
}
