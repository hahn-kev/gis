import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from "@angular/router";
import { Observable } from "rxjs/Observable";

@Injectable()
export class IsNewResolverService implements Resolve<boolean> {
  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    return route.params['name'] === 'new';
  }

  constructor() {
  }

}
