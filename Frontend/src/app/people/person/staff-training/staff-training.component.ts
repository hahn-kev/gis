import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { StaffTraining, StaffTrainingWithRequirement } from '../../training-requirement/staff-training';
import { TrainingRequirementService } from '../../training-requirement/training-requirement.service';
import { TrainingRequirement } from '../../training-requirement/training-requirement';
import { Observable, Subject, Subscription } from 'rxjs';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ConfirmDialogComponent } from '../../../dialog/confirm-dialog/confirm-dialog.component';
import * as moment from 'moment';
import { NgForm } from '@angular/forms';
import { LazyLoadService } from '../../../services/lazy-load.service';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-staff-training',
  templateUrl: './staff-training.component.html',
  styleUrls: ['./staff-training.component.scss']
})
export class StaffTrainingComponent implements OnInit, OnDestroy {

  private _staffId;
  public training: StaffTrainingWithRequirement[] = [];
  public requirements: Observable<TrainingRequirement[]>;
  public newTraining = new StaffTraining();
  public requirement: TrainingRequirement;
  public isNew: boolean;
  private subscription: Subscription;
  private staffIdSubject = new Subject<string>();
  @ViewChild('newForm') newForm: NgForm;

  constructor(lazyLoadService: LazyLoadService,
              private trainingService: TrainingRequirementService,
              private dialog: MatDialog,
              private snackBar: MatSnackBar) {
    this.requirements = lazyLoadService.share('requirements', () => this.trainingService.list());
    this.subscription = this.staffIdSubject.pipe(switchMap(
      staffId => this.trainingService.getTrainingByStaffId(staffId)))
      .subscribe(value => this.training = value);
  }


  get staffId() {
    return this._staffId;
  }

  @Input('staffId')
  set staffId(staffId: string) {
    this._staffId = staffId;
    this.staffIdSubject.next(staffId);
    this.isNew = !staffId;
  }

  ngOnInit() {

  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  async saveNew() {
    this.newTraining.staffId = this.staffId;
    this.newTraining.trainingRequirementId = this.requirement.id;
    let savedTraining = <StaffTrainingWithRequirement> await this.trainingService.saveStaffTraining(this.newTraining);
    savedTraining.requirementName = this.requirement.name;
    savedTraining.requirementScope = this.requirement.scope;
    this.training = [
      savedTraining,
      ...this.training
    ];

    this.newTraining = new StaffTraining();
    this.newForm.resetForm();
    this.snackBar.open(`Training Completed`, null, {duration: 2000});
  }

  async saveTraining(training: StaffTraining) {
    await this.trainingService.saveStaffTraining(training);
    this.snackBar.open(`Training Updated`, null, {duration: 2000});
  }

  async deleteTraining(training: StaffTrainingWithRequirement) {
    let confirm = await ConfirmDialogComponent.OpenWait(this.dialog,
      `Delete ${moment(training.completedDate).format('l')} ${training.requirementName} Training`,
      'Delete',
      'Cancel');
    if (!confirm) return;
    await this.trainingService.deleteStaffTraining(training.id);
    this.training = this.training.filter(value => value.id !== training.id);
    this.snackBar.open(`Training Deleted`, null, {duration: 2000});
  }
}
