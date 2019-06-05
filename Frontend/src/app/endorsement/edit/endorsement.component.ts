import { Component, OnInit } from '@angular/core';
import { Endorsement } from '../endorsement';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { BaseEditComponent } from '../../components/base-edit-component';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EndorsementService } from '../endorsement.service';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-endorsement',
  templateUrl: './endorsement.component.html',
  styleUrls: ['./endorsement.component.scss']
})
export class EndorsementComponent extends BaseEditComponent implements OnInit {
  endorsement: Endorsement;
  isNew: boolean;

  constructor(
    private endorsementService: EndorsementService,
    private route: ActivatedRoute,
    private location: Location,
    private snackBar: MatSnackBar,
    dialog: MatDialog) {
    super(dialog);
    this.route.data.subscribe(value => {
      this.endorsement = value.endorsement;
      this.isNew = !this.endorsement.id;
    });
  }

  ngOnInit() {
  }

  async save() {
    this.endorsement = await this.endorsementService.saveEndorsement(this.endorsement).toPromise();
    this.location.back();
    this.snackBar.open(`${this.endorsement.name} ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async deleteEndorsement() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog, 'Delete Certification?', 'Delete', 'Cancel');
    if (!result) return;
    await this.endorsementService.deleteEndorsement(this.endorsement.id).toPromise();
    this.location.back();
    this.snackBar.open(`${this.endorsement.name} Deleted`, null, {duration: 2000});
  }
}
