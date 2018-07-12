import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { EmergencyContactExtended } from '../../emergency-contact';
import { ActivatedRoute } from '@angular/router';
import { Person } from '../../person';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-emergency-contact',
  templateUrl: './emergency-contact.component.html',
  styleUrls: ['./emergency-contact.component.scss']
})
export class EmergencyContactComponent implements OnInit {
  @Input() customContactOnly: boolean;
  @Input() emergencyContact: EmergencyContactExtended;
  @Input() formId: string;
  @ViewChild('form') form: NgForm;
  public people: Person[];
  public isCustomContact;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value: {
      people: Person[]
    }) => {
      this.people = value.people.filter(person => person.id != this.emergencyContact.personId);
    });
    this.isCustomContact = !this.emergencyContact.contactId;
  }

  personAdded(person: Person) {
    this.people = [...this.people, person];
  }

  updateContactName(contactId: string) {
    let person = this.people.find(p => p.id == contactId);
    this.emergencyContact.contactPreferredName = person ? person.preferredName || person.firstName : '';
    this.emergencyContact.contactLastName = person ? person.lastName : '';
  }
}
