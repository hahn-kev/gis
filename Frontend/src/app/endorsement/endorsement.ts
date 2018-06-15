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

export class RequiredEndorsement extends BaseEntity {
  jobId: string;
  endorsementId: string;
}
