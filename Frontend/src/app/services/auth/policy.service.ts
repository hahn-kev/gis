import { Injectable } from '@angular/core';
import { UserToken } from '../../login/user-token';
import { LoginService } from './login.service';

@Injectable({
  providedIn: 'root'
})
export class PolicyService {

  private readonly policies: { [key: string]: ((user: UserToken) => boolean) };

  constructor(private loginService: LoginService) {
    this.policies = {
      viewSalary: (user: UserToken) => user.hasAnyRole(['admin', 'hradmin']),
      orgGroupManager: (user: UserToken) => user.hasAnyRole(['admin', 'hr']) || user.isHigherup,
      orgChart: (user: UserToken) => user.hasAnyRole(['admin', 'hr']) || user.isSupervisor,
      leaveManager: (user: UserToken) => user.hasAnyRole(['admin', 'hr']) || user.isHigherup,
      leaveSupervisor: (user: UserToken) => user.isLeaveDelegate || user.isSupervisor,
      endorsementManager: (user: UserToken) => user.hasAnyRole(['admin', 'hr']) || user.isHigherup,
      peopleEdit: (user: UserToken) => user.hasAnyRole(['admin', 'hr', 'registrar']) || user.isHigherup,
      missionOrgManager: (user: UserToken) => user.hasAnyRole(['admin', 'hr', 'registrar']) || user.isHigherup,
      hrEdit: (user: UserToken) => user.isHrOrAdmin() || user.isHigherup,
      addRoles: (user: UserToken) => user.isHrOrAdmin() || user.isHigherup,
      addEvaluations: (user: UserToken) => user.isHrOrAdmin() || user.isHigherup,
      addTraining: (user: UserToken) => user.isHrOrAdmin() || user.isHigherup,
      registrarEdit: (user: UserToken) => user.hasAnyRole(['admin', 'registrar']),
      userManager: (user: UserToken) => user.hasRole('admin'),
      impersonate: (user: UserToken) => user.hasRole('admin')
    };
  }

  getPolicy(policy: string): (user: UserToken) => boolean {
    return this.policies[policy];
  }

  testPolicy(policy: string) {
    let policyFunction = this.getPolicy(policy);
    return policyFunction && policyFunction(this.loginService.userToken);
  }
}
