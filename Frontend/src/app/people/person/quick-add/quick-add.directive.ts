import { Directive, EventEmitter, HostListener, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { QuickAddComponent } from './quick-add.component';
import { PersonWithStaff } from '../../person';
import { first } from 'rxjs/operators';

@Directive({
  selector: '[appQuickAdd]'
})
export class QuickAddDirective {

  @Output() updateList = new EventEmitter<PersonWithStaff>();
  @Output() updateSelected = new EventEmitter<string>();

  constructor(private dialog: MatDialog) {
  }

  @HostListener('click') onClick() {
    this.dialog.open(QuickAddComponent).afterClosed()
      .pipe(first())
      .subscribe((person: PersonWithStaff) => {
      //todo add to bound list
      if (person) {
        this.updateList.emit(person);
        this.updateSelected.emit(person.id);
      }
    });
  }
}
