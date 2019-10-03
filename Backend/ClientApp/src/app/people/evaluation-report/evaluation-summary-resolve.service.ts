import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { PersonEvluationSummary } from './person-evluation-summary';
import { Observable } from 'rxjs';
import { EvaluationService } from '../person/evaluation/evaluation.service';

@Injectable()
export class EvaluationSummaryResolveService implements Resolve<PersonEvluationSummary[]> {

  constructor(private evaluationService: EvaluationService) {
  }

  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<PersonEvluationSummary[]> | Promise<PersonEvluationSummary[]> | PersonEvluationSummary[] {
    return this.evaluationService.getSummaries();
  }
}
