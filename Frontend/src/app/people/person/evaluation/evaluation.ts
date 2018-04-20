import { BaseEntity } from '../../../classes/base-entity';

export class Evaluation extends BaseEntity {
  public personId: string;
  public roleId: string;
  public evaluator: string;
  public date: Date;
  public notes: string;
  public score: number;
  public total: number;
  public result: EvaluationResult;
}

export class EvaluationWithNames extends Evaluation {
  public jobTitle: string;
}

export enum EvaluationResult {
  Poor = 'Poor',
  Good = 'Good',
  Excelent = 'Excelent'
}
