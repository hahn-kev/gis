import { Injectable } from '@angular/core';
import { GradeService } from './grade.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Grade } from './grade';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class GradeListResolverService implements Resolve<Grade[]> {

  constructor(private gradeService: GradeService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Grade[]> | Promise<Grade[]> | Grade[] {
    return this.gradeService.list();
  }
}
