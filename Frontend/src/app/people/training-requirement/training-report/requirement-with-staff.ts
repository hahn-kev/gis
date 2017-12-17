import { TrainingRequirement } from '../training-requirement';
import { StaffWithName } from '../../person';
import { StaffTraining } from '../staff-training';

export class RequirementWithStaff {
  public requirement: TrainingRequirement;
  public staffsWithTraining: StaffWithTraining[];
  public completedRequirement: number;

  constructor(requirement: TrainingRequirement, staffsWithTraining: StaffWithTraining[]) {
    this.requirement = requirement;
    this.staffsWithTraining = staffsWithTraining;
    this.completedRequirement = this.staffsWithTraining.reduce((n, staffTraining) => n + (staffTraining.training.completedDate ? 1 : 0), 0);
  }
}

export class StaffWithTraining {
  public staff: StaffWithName;
  public training: StaffTraining;

  constructor(staff: StaffWithName, training: StaffTraining) {
    this.staff = staff;
    this.training = training || new StaffTraining(staff.id, null);
  }
}
