﻿export class Job {
  public id: string;
  public title: string;
  public type: string;
  public jobDescription: string;
  public orgGroupId: string;
  public current: boolean;
  public isStaff: boolean;
  public isDirector: boolean;
  public positions: number;

  constructor() {
    this.current = true;
  }
}