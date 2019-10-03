import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { PersonWithStaff } from '../../person';

@Component({
  selector: 'app-quick-add-button',
  templateUrl: './quick-add-button.component.html',
  styleUrls: ['./quick-add-button.component.scss']
})
export class QuickAddButtonComponent implements OnInit {
  @Input() caption = 'Person';
  @Input() personId: string;
  @Output() personIdChange = new EventEmitter<string>();
  @Output() updateList = new EventEmitter<PersonWithStaff>();

  constructor() {
  }

  ngOnInit() {
  }

}
