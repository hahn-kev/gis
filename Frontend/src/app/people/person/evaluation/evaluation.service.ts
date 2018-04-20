import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Evaluation } from './evaluation'

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
}
