export class User {
  public id: number;
  public userName: string;
  public phoneNumber: string;
  public email: string;
  public resetPassword = false;
  public personId: string;
  public personName: string;
  public roles: string[] = [];
  public sendHrLeaveEmails = false;
}
