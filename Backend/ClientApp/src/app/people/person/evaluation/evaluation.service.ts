import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Evaluation, EvaluationWithNames } from './evaluation';
import { PersonEvluationSummary } from '../../evaluation-report/person-evluation-summary';

@Injectable()
export class EvaluationService {

  constructor(private http: HttpClient) {
  }

  save(evaluation: Evaluation) {
    return this.http.post<Evaluation>('/api/evaluation', evaluation).toPromise();
  }

  deleteEvaluation(evaluationId) {
    return this.http.delete('/api/evaluation/' + evaluationId, {responseType: 'text'}).toPromise();
  }

  getSummaries() {
    return this.http.get<PersonEvluationSummary[]>('/api/evaluation/summaries');
  }

  getEvaluationsByPersonId(id: string) {
    return this.http.get<EvaluationWithNames[]>('/api/evaluation/person/' + id);
  }
}
