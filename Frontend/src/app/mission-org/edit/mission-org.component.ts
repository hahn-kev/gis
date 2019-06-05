import { Component, OnInit } from '@angular/core';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { MissionOrgStatus, MissionOrgWithYearSummaries } from '../mission-org';
import { MissionOrgService } from '../mission-org.service';
import { Person } from '../../people/person';
import { BaseEditComponent } from '../../components/base-edit-component';
import { MissionOrgLevel, MissionOrgYearSummary } from '../mission-org-year-summary';
import { Year } from '../../people/training-requirement/year';
import { RenderTemplateBottomSheetComponent } from '../../components/render-template-bottom-sheet/render-template-bottom-sheet.component';

@Component({
  selector: 'app-mission-org',
  templateUrl: './mission-org.component.html',
  styleUrls: ['./mission-org.component.scss']
})
export class MissionOrgComponent extends BaseEditComponent implements OnInit {
  public schoolYears = Year.years();
  public statusList = Object.keys(MissionOrgStatus);
  public levelList = Object.keys(MissionOrgLevel);
  public missionOrg: MissionOrgWithYearSummaries;
  public people: Person[];
  public isNew = false;
  public peopleInOrg: Person[];

  constructor(private missionOrgService: MissionOrgService,
              private route: ActivatedRoute,
              private router: Router,
              private snackBar: MatSnackBar,
              private bottomSheet: MatBottomSheet,
              dialog: MatDialog) {
    super(dialog);
  }

  ngOnInit() {
    this.route.data.subscribe((value) => {
      this.missionOrg = value.missionOrg;
      this.people = value.people;
      this.isNew = !this.missionOrg.id;
    });
  }

  createNewYear = () => {
    let yearSummary = new MissionOrgYearSummary();
    yearSummary.missionOrgId = this.missionOrg.id;
    return yearSummary;
  };

  async save() {
    await this.missionOrgService.save(this.missionOrg);
    this.router.navigate(['/mission-org/list']);
    this.snackBar.open(`${this.missionOrg.name} ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async delete() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog,
      `Delete Sending Org ${this.missionOrg.name}?`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.missionOrgService.delete(this.missionOrg.id);
    this.router.navigate(['/mission-org/list']);
    this.snackBar.open(`${this.missionOrg.name} Deleted`, null, {duration: 2000});
  }

  deleteYear = async (yearSummary: MissionOrgYearSummary) => {
    return await this.missionOrgService.deleteYear(yearSummary.id);
  };

  saveYear = async (yearSummary: MissionOrgYearSummary) => {
    return {...yearSummary, ... await this.missionOrgService.saveYear(yearSummary)};
  };

  async showStaff() {
    if (this.isNew) {
      this.peopleInOrg = [];
    } else if (!this.peopleInOrg) {
      this.peopleInOrg = await this.missionOrgService.listPeople(this.missionOrg.id).toPromise();
    }
    RenderTemplateBottomSheetComponent.Open(this.bottomSheet, 'staff');
  }
}
