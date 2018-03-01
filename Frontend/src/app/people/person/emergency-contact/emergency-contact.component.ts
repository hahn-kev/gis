import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
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
  @Input() emergencyContact: EmergencyContactExtended;
  @Input() formId: string;
  @Output() submit = new EventEmitter();
  @ViewChild('form') form: NgForm;
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
    let person = this.people.find(person => person.id == contactId);
    this.emergencyContact.contactPreferedName = person ? person.preferredName : '';
  }
}
