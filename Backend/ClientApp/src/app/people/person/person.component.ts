import { Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { ActivatedRoute, NavigationExtras, Router } from '@angular/router';
import { Person, PersonWithOthers } from '../person';
import { PersonService } from '../person.service';
import { RoleWithJob } from '../role';
import { OrgGroup } from '../groups/org-group';
import { MatDialog } from '@angular/material/dialog';
import { MatExpansionPanel } from '@angular/material/expansion';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { NgForm, NgModel } from '@angular/forms';
import { EmergencyContactExtended } from '../emergency-contact';
import { EmergencyContactComponent } from './emergency-contact/emergency-contact.component';
import { RoleComponent } from './role.component';
import { countries } from '../countries';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Job } from '../../job/job';
import { MissionOrg } from '../../mission-org/mission-org';
import { GroupService } from '../groups/group.service';
import { OrgChainLink } from '../groups/org-chain';
import { CanComponentDeactivate } from '../../services/can-deactivate.guard';
import { StaffWithOrgName } from '../staff';
import { LazyLoadService } from '../../services/lazy-load.service';
import { MissionOrgService } from '../../mission-org/mission-org.service';
import { LeaveType } from '../self/self';
import { Location } from '@angular/common';
import { EvaluationWithNames } from './evaluation/evaluation';
import { EvaluationService } from './evaluation/evaluation.service';
import { EvaluationComponent } from './evaluation/evaluation.component';
import { LoginService } from '../../services/auth/login.service';
import { first } from 'rxjs/internal/operators';
import { Donor } from '../donor';
import { EndorsementService } from '../../endorsement/endorsement.service';
import { Endorsement, StaffEndorsementWithName } from '../../endorsement/endorsement';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html',
  styleUrls: ['./person.component.scss'],
  providers: [LazyLoadService]
})
export class PersonComponent implements OnInit, CanComponentDeactivate {
  public isAdmin: boolean;
  public isNew: boolean;
  public isSelf: boolean;
  public filteredCountries: Observable<string[]>;
  public person: PersonWithOthers;
  public groups: Observable<OrgGroup[]>;
  public missionOrgs: Observable<MissionOrg[]>;
  public endorsements: Observable<Endorsement[]>;
  public people: Person[];
  public peopleExcludingSelf: Person[];
  public jobs: { [key: string]: Job };
  public peopleMap: { [key: string]: Person } = {};
  public newEmergencyContact = new EmergencyContactExtended();
  public newRole = new RoleWithJob();
  public newEvaluation = new EvaluationWithNames();
  public staffEndorsments: Array<string> = [];
  public staffInsurer: string[] = [];
  @ViewChildren(NgForm) forms: QueryList<NgForm>;
  @ViewChild('newEmergencyContactEl', {static: false}) newEmergencyContactEl: EmergencyContactComponent;
  @ViewChild('newRoleEl', {static: false}) newRoleEl: RoleComponent;
  @ViewChild('countriesControl', {static: true}) countriesControl: NgModel;

  constructor(private route: ActivatedRoute,
              private personService: PersonService,
              private groupService: GroupService,
              missionOrgService: MissionOrgService,
              loginService: LoginService,
              private evaluationService: EvaluationService,
              private endorsementService: EndorsementService,
              private router: Router,
              private dialog: MatDialog,
              private snackBar: MatSnackBar,
              private location: Location,
              private lazyLoadService: LazyLoadService) {
    loginService.safeUserToken().pipe(first()).subscribe(value => this.isAdmin = value.hasRole('admin'));
    this.isSelf = this.router.url.indexOf('self') != -1;
    this.groups = this.lazyLoadService.share('orgGroups', () => this.groupService.getAll());
    this.missionOrgs = this.lazyLoadService.share('missionOrgs', () => missionOrgService.list());
    this.endorsements = this.lazyLoadService.share('endorsements', () => this.endorsementService.list());
    this.route.data.subscribe((value: {
      person: PersonWithOthers,
      people: Person[]
    }) => {
      this.person = value.person;
      if (this.person.leaveDetails) {
        this.person.leaveDetails.leaveUsages = [
          ...this.person.leaveDetails.leaveUsages
            .filter(l => l.leaveType != LeaveType.Other)
            .sort((a, b) => a.leaveType.localeCompare(b.leaveType)),
          this.person.leaveDetails.leaveUsages.find(l => l.leaveType == LeaveType.Other)
        ];
      }

      if (value.person.staff) {
        this.staffEndorsments = (value.person.staff.endorsements || '').split(',');
        this.staffInsurer = (value.person.staff.insurer || '').split(',');
      }
      this.isNew = !this.person.id;
      this.newRole.personId = this.person.id;
      this.newEmergencyContact.personId = this.person.id;
      this.newEvaluation.personId = this.person.id;
      this.people = value.people;
      this.peopleExcludingSelf = value.people.filter(person => person.id != value.person.id);
      this.peopleMap = this.people.reduce((pMap, currentValue) => {
        pMap[currentValue.id] = currentValue;
        return pMap;
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

  trackLinksBy(index: number, link: OrgChainLink) {
    return link.id;
  }

  async isStaffChanged(isStaff: boolean, isStaffElement: NgModel): Promise<void> {
    if (isStaff) {
      //just a guard in case this gets called incorrectly
      this.person.staff = this.person.staff || new StaffWithOrgName();
      return;
    }
    //deleting?
    if (!this.isNew && this.person.staff.id) {
      let result = false;
      if (this.isAdmin) {
        result = await ConfirmDialogComponent.OpenWait(
          this.dialog,
          `Deleting staff, data will be lost, this can not be undone`,
          'Delete',
          'Cancel');
      } else {
        this.snackBar.open(`Only an Admin can mark someone as staff as not staff, this will delete data`, 'Dismiss');
      }
      if (!result) {
        //roll back switch
        isStaffElement.control.setValue(true, {emitViewToModelChange: false});
        return;
      }
    }
    this.person.staff = null;
  }

  async isDonorChanged(isDonor: boolean, isDonorElement: NgModel) {
    if (isDonor) {
      //just a guard in case this gets called incorrectly
      this.person.donor = this.person.donor || new Donor();
      return;
    }
    if (!this.isNew && this.person.donor.id) {
      let result = false;
      if (this.isAdmin) {
        result = await ConfirmDialogComponent.OpenWait(
          this.dialog,
          `Deleting donor, data will be lost, this can not be undone`,
          'Delete',
          'Cancel');
      } else {
        this.snackBar.open(`Only an Admin can mark someone as not a donor, this will delete data`, 'Dismiss');
      }
      if (!result) {
        //roll back switch
        isDonorElement.control.setValue(true, {emitViewToModelChange: false});
        return;
      }
    }
    this.person.donor = null;
  }

  async save(): Promise<void> {
    let extras: NavigationExtras = {};
    let savedPerson = await this.personService.updatePerson(this.person, this.isSelf);
    this.snackBar.open(`${savedPerson.preferredName} Saved`, null, {duration: 2000});
    let commands: Array<any> = ['/people/list'];
    if (this.isNew) {
      commands = ['people', 'edit', savedPerson.id];
      extras.replaceUrl = true;
    } else if (this.isSelf) {
      commands = ['home'];
    } else {
      this.location.back();
      return;
    }
    this.router.navigate(commands, extras);
  }

  async deletePerson() {
    let result = await ConfirmDialogComponent.OpenWait(
      this.dialog,
      `Delete person?`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.personService.deletePerson(this.person.id);
    this.router.navigate(['/people/list']);
    this.snackBar.open(`${this.person.preferredName} Deleted`, null, {duration: 2000});
  }

  async saveRole(role: RoleWithJob, panel: MatExpansionPanel, isNew = false): Promise<void> {
    let updatedRole = await this.personService.updateRole(role);
    if (isNew) {
      role = {...updatedRole, ...role};
      this.person.roles = [...this.person.roles, <RoleWithJob> role];
      if (role.active && this.person.staff) {
        //back end will have done this already, just updating the front end
        this.person.staff.orgGroupId = role.job.orgGroupId;
      }
      this.newRole = new RoleWithJob();
      this.newRole.personId = this.person.id;
      this.newRoleEl.form.resetForm();
      this.snackBar.open(`Role Added`, null, {duration: 2000});
    } else {
      this.snackBar.open(`Role Saved`, null, {duration: 2000});
    }
    panel.close();
  }

  async deleteRole(role: RoleWithJob) {
    let result = await ConfirmDialogComponent.OpenWait(
      this.dialog,
      `Delete role ${role.job.title}?`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.personService.deleteRole(role.id);
    this.person.roles = this.person.roles.filter(value => value.id != role.id);
    this.snackBar.open(`Role Deleted`, null, {duration: 2000});

  }

  async saveEvaluation(evaluation: EvaluationWithNames,
                       panel: MatExpansionPanel,
                       evalComponent: EvaluationComponent,
                       isNew = false) {
    let updatedEval = await this.evaluationService.save(evaluation);
    if (isNew) {
      evaluation = {...evaluation, ...updatedEval};
      this.person.evaluations = [...this.person.evaluations, evaluation];
      this.newEvaluation = new EvaluationWithNames();
      this.newEvaluation.personId = this.person.id;
      evalComponent.form.resetForm();
      this.snackBar.open(`Evaluation Added`, null, {duration: 2000});
    } else {
      this.snackBar.open(`Evaluation Saved`, null, {duration: 2000});
    }
    panel.close();
  }

  async deleteEvaluation(evaluation: EvaluationWithNames) {
    let result = await ConfirmDialogComponent.OpenWait(
      this.dialog,
      `Delete ${evaluation.jobTitle} Evaluation?`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.evaluationService.deleteEvaluation(evaluation.id);
    this.person.evaluations = this.person.evaluations.filter(value => value.id != evaluation.id);
    this.snackBar.open(`Evaluation Deleted`, null, {duration: 2000});
  }

  canDeactivate() {
    if (!this.forms.some(value => !value.pristine && !value.submitted)) return true;
    return ConfirmDialogComponent.OpenWait(this.dialog, 'Discard Changes?', 'Discard', 'Cancel');
  }

  createNewStaffEndorsement = () => new StaffEndorsementWithName(this.person.id);
  saveStaffEndorsement = async (item: StaffEndorsementWithName) => {
    return {...item, ... await this.endorsementService.saveStaffEndorsement(item)};
  }

  deleteStaffEndorsement = (item: StaffEndorsementWithName) => {
    return this.endorsementService.deleteStaffEndorsement(item.id);
  }
}
