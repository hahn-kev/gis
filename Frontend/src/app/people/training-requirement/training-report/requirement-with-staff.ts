import { TrainingRequirement } from '../training-requirement';
import { StaffWithName, StaffWithRoles } from '../../staff';
import { StaffTraining } from '../staff-training';
import { RoleWithJob } from '../../role';
import { OrgGroup } from '../../groups/org-group';

export class RequirementWithStaff {
  public requirement: TrainingRequirement;
  public staffsWithTraining: StaffWithTraining[];
  public completedRequirement: number;
  public totalStaff: number;
  public orgGroup: OrgGroup;

  constructor(requirement: TrainingRequirement,
              staffsWithTraining: StaffWithTraining[],
              orgGroup: OrgGroup,
              totalStaff: number) {
    this.requirement = requirement;
    this.staffsWithTraining = staffsWithTraining;
    this.orgGroup = orgGroup;
    this.completedRequirement = this.staffsWithTraining.reduce((n,
                                                                staffTraining) => n + (staffTraining.training.completedDate ?
      1 :
      0), 0);
    this.totalStaff = totalStaff;
    if (totalStaff > this.staffsWithTraining.length) {
      //staff with training has been filtered, the missing staff have completed the training
      this.completedRequirement = totalStaff - this.staffsWithTraining.length;
    }
  }
}

export class StaffWithTraining {
  public selected = false;
  public staff: StaffWithName;
  public roles: RoleWithJob[];
  public training: StaffTraining;

  constructor(staff: StaffWithRoles, training: StaffTraining) {
    this.staff = staff.staffWithName;
    this.roles = staff.personRolesWithJob;
    this.training = training || StaffTraining.New(staff.staffWithName.id);
  }
}
