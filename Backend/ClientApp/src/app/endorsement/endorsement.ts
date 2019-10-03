import { BaseEntity } from '../classes/base-entity';

export class Endorsement extends BaseEntity {
  name: string;
}

export class StaffEndorsement extends BaseEntity {
  personId: string;
  endorsementId: string;
  endorsementDate: Date;
  agency: string;
}

export class StaffEndorsementWithName extends StaffEndorsement {
  constructor(personId: string) {
    super();
    this.personId = personId;
  }

  endorsementName: string;
}

export class RequiredEndorsement extends BaseEntity {
  jobId: string;
  endorsementId: string;
}

export class RequiredEndorsementWithName extends RequiredEndorsement {
  constructor(jobId: string) {
    super();
    this.jobId = jobId;
  }

  endorsementName: string;
}
