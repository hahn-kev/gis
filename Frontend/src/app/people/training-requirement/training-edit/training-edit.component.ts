import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TrainingRequirementService } from '../training-requirement.service';
import { TrainingRequirement } from '../training-requirement';
import { Year } from '../year';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ConfirmDialogComponent } from '../../../dialog/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-training-edit',
  templateUrl: './training-edit.component.html',
  styleUrls: ['./training-edit.component.scss']
})
export class TrainingEditComponent implements OnInit {
  public training: TrainingRequirement;
  public years: Year[];
  public isNew = false;

  constructor(private route: ActivatedRoute,
              private trainingService: TrainingRequirementService,
              private router: Router,
              private dialog: MatDialog,
              private snackBar: MatSnackBar) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value: { training: TrainingRequirement }) => {
      this.training = value.training;
      this.isNew = !this.training.id;
      if (!this.training.id) {
        this.training.firstYear = new Date().getUTCFullYear();
        this.training.scope = 'AllStaff';
      }
    });
    this.years = this.trainingService.years();
  }

  async save(): Promise<void> {
    await this.trainingService.save(this.training);
    this.router.navigate(['training', 'list']);
    this.snackBar.open(`Training Requirement ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async delete() {
    const dialogRef = this.dialog.open(ConfirmDialogComponent,
      {
        data: ConfirmDialogComponent.Options(`Deleting Training requirement, any training records will also be deleted`,
          'Delete',
          'Cancel')
      });
    const result = await dialogRef.afterClosed().toPromise();
    if (!result) return;
    await this.trainingService.deleteRequirement(this.training.id);
    this.router.navigate(['training', 'list'], {relativeTo: this.route});
    this.snackBar.open(`Training Requirement Deleted`, null, {duration: 2000});
  }

}
