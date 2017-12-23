import { Component, Input, OnInit } from '@angular/core';
import { StaffTraining, StaffTrainingWithRequirement } from '../../training-requirement/staff-training';
import { TrainingRequirementService } from '../../training-requirement/training-requirement.service';
import { TrainingRequirement } from '../../training-requirement/training-requirement';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/switchMap';
import { Subscription } from 'rxjs/Subscription';

@Component({
  selector: 'app-staff-training',
  templateUrl: './staff-training.component.html',
  styleUrls: ['./staff-training.component.scss']
})
export class StaffTrainingComponent implements OnInit {

  private _staffId;
  public training: StaffTrainingWithRequirement[] = [];
  public requirements: Map<string, TrainingRequirement>;
  public requirementsList: TrainingRequirement[];
  public newTraining = new StaffTraining();
  public requirement: TrainingRequirement;
  private subscription: Subscription;

  constructor(private trainingService: TrainingRequirementService) {
    this.subscription = this.staffIdSubject.switchMap(staffId => this.trainingService.getTrainingByStaffId(staffId))
      .combineLatest(this.trainingService.listMapped()).subscribe(this.updateTrainingList.bind(this));
  }

  staffIdSubject = new Subject<string>();

  get staffId() {
    return this._staffId;
  }

  @Input('staffId')
  set staffId(staffId: string) {
    this._staffId = staffId;
    this.staffIdSubject.next(staffId);
  }

  updateTrainingList([training, requirements]: [StaffTraining[], Map<string, TrainingRequirement>]) {
    this.requirements = requirements;
    this.requirementsList = Array.from(requirements.values());
    this.training = training.map(this.includeRequirementsInTraining.bind(this));
  }

  includeRequirementsInTraining(value: StaffTraining): StaffTrainingWithRequirement {
    let staffTraining = <StaffTrainingWithRequirement> value;
    let requirement = this.requirements.get(value.trainingRequirementId);
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
    let savedTraining = await this.saveTraining(this.newTraining);
    this.training = [
      this.includeRequirementsInTraining(savedTraining),
      ...this.training
    ];

    this.newTraining = new StaffTraining();
  }

  async saveTraining(training: StaffTraining) {
    return await this.trainingService.saveStaffTraining(training);
  }
}
