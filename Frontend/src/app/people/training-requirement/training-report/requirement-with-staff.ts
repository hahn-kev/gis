import { TrainingRequirement } from '../training-requirement';
import { StaffWithName } from '../../person';
import { StaffTraining } from '../staff-training';

export class RequirementWithStaff {
  public requirement: TrainingRequirement;
  public staffsWithTraining: StaffWithTraining[];
  public completedRequirement: number;
  public totalStaff: number;

  constructor(requirement: TrainingRequirement, staffsWithTraining: StaffWithTraining[], totalStaff: number) {
    this.requirement = requirement;
    this.staffsWithTraining = staffsWithTraining;
    this.completedRequirement = this.staffsWithTraining.reduce((n, staffTraining) => n + (staffTraining.training.completedDate ? 1 : 0), 0);
    this.totalStaff = totalStaff;
    if (totalStaff > this.staffsWithTraining.length) {
      //staff with training has been filtered, the missing staff have completed the training
      this.completedRequirement = totalStaff - this.staffsWithTraining.length;
    }
  }
}

export class StaffWithTraining {
  public staff: StaffWithName;
  public training: StaffTraining;

  constructor(staff: StaffWithName, training: StaffTraining) {
    this.staff = staff;
    this.training = training || StaffTraining.New(staff.id);
  }
}
