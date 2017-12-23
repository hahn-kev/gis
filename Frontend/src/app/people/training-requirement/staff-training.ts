export class StaffTraining {

  public id: string;
  public staffId: string;
  public trainingRequirementId: string;
  public completedDate: Date;

  static getKey(staffTraining: StaffTraining) {
    return staffTraining.staffId + '_' + staffTraining.trainingRequirementId;
  }
}

export class StaffTrainingWithRequirement extends StaffTraining {
  public requirementName: string;
  public requirementScope: string;
}
