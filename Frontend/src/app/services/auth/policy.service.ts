import { Injectable } from '@angular/core';
import { UserToken } from '../../login/user-token';

@Injectable({
  providedIn: 'root'
})
export class PolicyService {

  private readonly policies: { [key: string]: ((user: UserToken) => boolean) };

  constructor() {
    this.policies = {
      viewSalary: (user: UserToken) => user.hasAnyRole(['admin', 'hradmin']),
      orgGroupManager: (user: UserToken) => user.hasAnyRole(['admin', 'hradmin']) || user.isHigherup,
      orgChart: (user: UserToken) => user.hasAnyRole(['admin', 'hr']) || user.isSupervisor,
      leaveManager: (user: UserToken) => user.hasAnyRole(['admin', 'hr']) || user.isHigherup,
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

  getPolicy(policy: string) {
    return this.policies[policy];
  }
}
