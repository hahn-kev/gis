import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { EmergencyContactExtended } from '../../emergency-contact';
import { ActivatedRoute } from '@angular/router';
import { Person } from '../../person';
import { PersonService } from '../../person.service';

@Component({
  selector: 'app-emergency-contact',
  templateUrl: './emergency-contact.component.html',
  styleUrls: ['./emergency-contact.component.scss']
})
export class EmergencyContactComponent implements OnInit {
  @Input() customContactOnly: boolean;
  public people: Person[];
  @Input() personId: string;
  @Input() emergencyContacts: EmergencyContactExtended[];
  @Output() emergencyContactsChange = new EventEmitter<EmergencyContactExtended[]>();

  constructor(private route: ActivatedRoute, private personService: PersonService) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value: {
      people: Person[]
    }) => {
      this.people = value.people.filter(person => person.id != this.personId);
    });
  }

  personAdded(person: Person) {
    this.people = [...this.people, person];
  }

  updateContactName(ec: EmergencyContactExtended, contact: Person) {
    ec.contactPreferredName = contact.preferredName || contact.firstName || '';
    ec.contactLastName = contact.lastName || '';
    ec.contactPhone = contact.phoneNumber;
    ec.contactEmail = contact.email;
  }

  createNewContact = () => {
    let newEmergencyContact = new EmergencyContactExtended();
    newEmergencyContact.personId = this.personId;
    newEmergencyContact.order = this.emergencyContacts.length + 1;
    return newEmergencyContact;
  };

  saveEmergencyContact = (emergencyContact: EmergencyContactExtended) => {
    return this.personService.updateEmergencyContact(emergencyContact);

    // this.person.emergencyContacts.sort((a, b) => a.order - b.order);
  };

  deleteEmergencyContact = (emergencyContact: EmergencyContactExtended) => {
    return this.personService.deleteEmergencyContact(emergencyContact.id);
  };

  filterAvaliblePeople(ec: EmergencyContactExtended, people: Person[], contacts: EmergencyContactExtended[]) {
    return people.filter(value => {
      if (value.id == ec.contactId) return true;
      return !contacts.some(contact => contact.contactId == value.id);
    });
  }
}
