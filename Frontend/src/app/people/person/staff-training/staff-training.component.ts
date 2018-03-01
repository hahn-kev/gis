import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { StaffTraining, StaffTrainingWithRequirement } from '../../training-requirement/staff-training';
import { TrainingRequirementService } from '../../training-requirement/training-requirement.service';
import { TrainingRequirement } from '../../training-requirement/training-requirement';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/switchMap';
import { Subscription } from 'rxjs/Subscription';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ConfirmDialogComponent } from '../../../dialog/confirm-dialog/confirm-dialog.component';
import * as moment from 'moment';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-staff-training',
  templateUrl: './staff-training.component.html',
  styleUrls: ['./staff-training.component.scss']
})
export class StaffTrainingComponent implements OnInit, OnDestroy {

  private _staffId;
  public training: StaffTrainingWithRequirement[] = [];
  public requirements: Map<string, TrainingRequirement>;
  public requirementsList: TrainingRequirement[];
  public newTraining = new StaffTraining();
  public requirement: TrainingRequirement;
  public isNew: boolean;
  private subscription: Subscription;
  private staffIdSubject = new Subject<string>();
  @ViewChild('newForm') newForm: NgForm;

  constructor(private trainingService: TrainingRequirementService,
              private dialog: MatDialog,
              private snackBar: MatSnackBar) {
    this.subscription = this.staffIdSubject.switchMap(staffId => this.trainingService.getTrainingByStaffId(staffId))
      .combineLatest(this.trainingService.listMapped()).subscribe(this.updateTrainingList.bind(this));
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

  updateTrainingList([training, requirements]: [StaffTraining[], Map<string, TrainingRequirement>]) {
    this.requirements = requirements;
    this.requirementsList = Array.from(requirements.values());
    this.training = training.map(this.includeRequirementsInTraining.bind(this));
  }

  includeRequirementsInTraining(value: StaffTraining): StaffTrainingWithRequirement {
    const staffTraining = <StaffTrainingWithRequirement> value;
    const requirement = this.requirements.get(value.trainingRequirementId);
    staffTraining.requirementName = requirement.name;
    staffTraining.requirementScope = requirement.scope;
    return staffTraining;
  }

  ngOnInit() {

  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  async saveNew() {
    this.newTraining.staffId = this.staffId;
    this.newTraining.trainingRequirementId = this.requirement.id;
    const savedTraining = await this.trainingService.saveStaffTraining(this.newTraining);
    this.training = [
      this.includeRequirementsInTraining(savedTraining),
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
