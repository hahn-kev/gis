import { BaseEntity } from '../../../classes/base-entity';

export class Education extends BaseEntity {
  personId: string;
  degree: Degree;
  field: string;
  institution: string;
  country: string;
  completedDate: string;
}

export enum Degree {
  Diploma = 'Diploma',
  Associates = 'Associates',
  Bachelors = 'Bachelors',
  PGCE = 'PGCE',
  Masters = 'Masters',
  Doctorate = 'Doctorate',
  Other = 'Other'
}

export function formatDegree(degree: Degree): string {
  if (degree == Degree.Associates) return 'Associates/HND';
  return degree;
}
