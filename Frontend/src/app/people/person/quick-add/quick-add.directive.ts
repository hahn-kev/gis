import { Directive, EventEmitter, HostListener, Output } from '@angular/core';
import { MatDialog } from '@angular/material';
import { QuickAddComponent } from './quick-add.component';
import { PersonWithStaff } from '../../person';

@Directive({
  selector: '[appQuickAdd]'
})
export class QuickAddDirective {

  constructor(private dialog: MatDialog) {
  }

  @Output() updateList = new EventEmitter<PersonWithStaff>();
  @Output() updateSelected = new EventEmitter<string>();

  @HostListener('click') onClick() {
    this.dialog.open(QuickAddComponent).afterClosed().subscribe((person: PersonWithStaff) => {
      //todo add to bound list
      if (person) {
        this.updateList.emit(person);
        this.updateSelected.emit(person.id);
      }
    });
  }
}
