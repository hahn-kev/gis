export class TrainingRequirement {

  constructor(public id?: string,
    public name?: string,
    public firstYear?: number,
    public lastYear?: number,
    public scope?: string,
    public departmentId?: string) {
  }
}
