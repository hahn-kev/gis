import { Component, OnInit } from '@angular/core';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { MissionOrg } from '../mission-org';
import { MissionOrgService } from '../mission-org.service';
import { Person } from '../../people/person';
import { BaseEditComponent } from '../../components/base-edit-component';

@Component({
  selector: 'app-mission-org',
  templateUrl: './mission-org.component.html',
  styleUrls: ['./mission-org.component.scss']
})
export class MissionOrgComponent extends BaseEditComponent implements OnInit {

  public missionOrg: MissionOrg;
  public people: Person[];
  public isNew = false;

  constructor(private missionOrgService: MissionOrgService,
              private route: ActivatedRoute,
              private router: Router,
              private snackBar: MatSnackBar,
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

  async save() {
    await this.missionOrgService.save(this.missionOrg);
    this.router.navigate(['/mission-org/list']);
    this.snackBar.open(`${this.missionOrg.name} ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async delete() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog, `Delete Mission Org ${this.missionOrg.name}?`, 'Delete', 'Cancel');
    if (!result) return;
    await this.missionOrgService.delete(this.missionOrg.id);
    this.router.navigate(['/mission-org/list']);
    this.snackBar.open(`${this.missionOrg.name} Deleted`, null, {duration: 2000});
  }
}
