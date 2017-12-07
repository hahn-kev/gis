import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PersonExtended } from '../person';
import { PersonService } from '../person.service';
import { Role } from '../role';
import { OrgGroup } from '../groups/org-group';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html',
  styleUrls: ['./person.component.scss']
})
export class PersonComponent implements OnInit {
  public person: PersonExtended;
  public groups: OrgGroup[];
  public newRole = new Role();

  constructor(private route: ActivatedRoute,
    private personService: PersonService,
    private router: Router) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value: { person: PersonExtended, groups: OrgGroup[] }) => {
      this.person = value.person;
      this.groups = value.groups;
      this.newRole.personId = this.person.id;
    });
  }

  async save(): Promise<void> {
    await this.personService.updatePerson(this.person);
    this.router.navigate(['/people']);
  }

  async saveRole(role: Role, isNew = false): Promise<void> {
    role = await this.personService.updateRole(role);
    if (isNew) {
      this.person.roles = [...this.person.roles, role];
      this.newRole = new Role();
      this.newRole.personId = this.person.id;
    }
  }
}
