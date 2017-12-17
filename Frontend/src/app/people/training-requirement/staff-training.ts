export class StaffTraining {

  constructor(staffId?: string, completedDate?: Date, trainingRequirementId?: string) {
    this.staffId = staffId;
    this.trainingRequirementId = trainingRequirementId;
    this.completedDate = completedDate;
  }

  public staffId: string;
  public trainingRequirementId: string;
  public completedDate: Date;

  static getKey(staffTraining: StaffTraining) {
    return staffTraining.staffId + '_' + staffTraining.trainingRequirementId;
  }
}
