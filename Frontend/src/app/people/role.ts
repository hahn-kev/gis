export class Role {

  constructor(public id?: string,
    public name?: string,
    public startDate?: Date,
    public active?: boolean,
    public endDate?: Date,
    public personId?: string) {
  }
}
