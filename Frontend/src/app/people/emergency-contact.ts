export class EmergencyContact {
  public id: string;
  public personId: string;
  public contactId: string;
  public order: number;
}

export class EmergencyContactExtended extends EmergencyContact {
  public contactPreferedName: string;
}
