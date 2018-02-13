import { Component, Input, OnInit } from '@angular/core';
import { EmergencyContactExtended } from '../../emergency-contact';
import { ActivatedRoute } from '@angular/router';
import { Person } from '../../person';

@Component({
  selector: 'app-emergency-contact',
  templateUrl: './emergency-contact.component.html',
  styleUrls: ['./emergency-contact.component.scss']
})
export class EmergencyContactComponent implements OnInit {
  @Input() emergencyContact: EmergencyContactExtended;
  public people: Person[];

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value: {
      people: Person[]
    }) => {
      this.people = value.people.filter(person => person.id != this.emergencyContact.personId);
    });
  }

  updateContactName(contactId: string) {
    this.emergencyContact.contactPreferedName = this.people.find(person => person.id == contactId).preferredName;
  }
}
