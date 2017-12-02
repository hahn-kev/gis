export class OrgGroup {

  constructor(public groupName?: string,
              public type?: string,
              public supervisor?: string,
              public parentId?: string,
              public id?: string) {
  }
}
