export class TrainingRequirement {
  public id: string;
  public name: string;
  public firstYear: number;
  public lastYear: number;
  public scope: string;
  public departmentId: string;
  public renewMonthsCount: number;
  constructor (){
    this.renewMonthsCount = 12;
  }
}
