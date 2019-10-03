import { OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { LoginService } from './login.service';
import { UserToken } from '../../login/user-token';
import { Subscription } from 'rxjs';
import { PolicyService } from './policy.service';

@Pipe({
  name: 'isUserPolicy',
  pure: true
})
export class IsUserPolicyPipe implements PipeTransform, OnDestroy {
  userToken: UserToken;
  sub: Subscription;

  constructor(loginService: LoginService, private policyService: PolicyService) {
    this.sub = loginService.currentUserToken()
      .subscribe(value => {
        this.userToken = value;
      });
  }

  transform(value: string, args?: any): boolean {
    if (this.userToken == null) return false;
    return this.policyService.getPolicy(value)(this.userToken);
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

}
