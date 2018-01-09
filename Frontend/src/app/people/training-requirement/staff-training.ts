export class StaffTraining {
  public id: string;
  public staffId: string;
  public trainingRequirementId: string;
  public completedDate: Date;

  static getKey(staffTraining: StaffTraining) {
    return staffTraining.staffId + '_' + staffTraining.trainingRequirementId;
  }

  static New(staffId: string, trainingId?: string) {
    let training = new StaffTraining();
    training.staffId = staffId;
    training.trainingRequirementId = trainingId;
    return training;
  }
}

export class StaffTrainingWithRequirement extends StaffTraining {
  public requirementName: string;
  public requirementScope: string;
}
