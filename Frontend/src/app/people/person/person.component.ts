import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PersonExtended } from '../person';
import { PersonService } from '../person.service';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html',
  styleUrls: ['./person.component.scss']
})
export class PersonComponent implements OnInit {
  public person: PersonExtended;

  constructor(private route: ActivatedRoute,
              private personService: PersonService,
              private router: Router) {
  }

  ngOnInit() {
    this.route.data.subscribe((value: { person: PersonExtended }) => {
      this.person = value.person;
    })
  }

  async save() {
    await this.personService.update(this.person);
    this.router.navigate(['/person']);
  }
}
