import { Injectable } from '@angular/core';
import { GradeService } from './grade.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Grade } from './grade';
import { Observable } from 'rxjs';

@Injectable()
export class GradeResolverService implements Resolve<Grade>{

  constructor(private gradeService: GradeService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Grade> | Promise<Grade> | Grade {
    if (route.params['id'] == 'new') return new Grade();
    return this.gradeService.get(route.params['id']);
  }

}
