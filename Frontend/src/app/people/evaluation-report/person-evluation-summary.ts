import { PersonWithStaff } from '../person';

export class PersonEvluationSummary {
  person: PersonWithStaff;
  evaluations: number;
  goodEvaluations: number;
  poorEvaluations: number;
  excellentEvaluations: number;
  averagePercentage: number;
}
