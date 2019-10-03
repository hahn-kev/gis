import { BaseEntity } from '../../classes/base-entity';

export class Grade extends BaseEntity {
  public gradeNo: number;
  public minPay: number;
  public maxPay: number;
}
