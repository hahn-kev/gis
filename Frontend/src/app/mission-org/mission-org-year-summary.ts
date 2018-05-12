import { BaseEntity } from '../classes/base-entity';
import { MissionOrgStatus } from './mission-org';

export class MissionOrgYearSummary extends BaseEntity {
  public year: number;
  public studentCount: number;
  public teacherCount: number;
  public status?: MissionOrgStatus;
  public level?: MissionOrgLevel;
}

export enum MissionOrgLevel {
  Bronze = 'Bronze',
  Silver = 'Silver',
  Gold = 'Gold',
  Platinum = 'Platinum'
}
