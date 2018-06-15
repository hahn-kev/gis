import { Component, EventEmitter, Input, OnInit, Output, QueryList, TemplateRef, ViewChildren } from '@angular/core';
import { AccordionListContentDirective } from './accordion-list-content.directive';
import { AccordionListFormDirective } from './accordion-list-form.directive';
import { MatDialog, MatExpansionPanel, MatSnackBar } from '@angular/material';
import { NgForm } from '@angular/forms';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-accordion-list',
  templateUrl: './accordion-list.component.html',
  styleUrls: ['./accordion-list.component.scss']
})
export class AccordionListComponent<T> implements OnInit {
  @Input() title: string = null;
  @Input() createNewItem: () => T;
  newItem: T;
  @Input() itemTitle = 'Item';
  @Input() addNew = true;
  @Input() showActions = true;
  @Input() items: T[] = [];
  @Output() itemsChange = new EventEmitter<T[]>();

  @Input() save: (item: T) => Promise<T>;
  @Input() delete: (item: T) => Promise<boolean>;
  @ViewChildren(MatExpansionPanel) expansionPanels: QueryList<MatExpansionPanel>;
  newForm: AccordionListFormDirective<T>;
  forms: AccordionListFormDirective<T>[] = [];
  public header: TemplateRef<{ $implicit: T, index: number }>;
  public content: AccordionListContentDirective<T>;

  constructor(private snackBar: MatSnackBar, private dialog: MatDialog) {
  }

  async onDelete(item: T) {
    let result = await ConfirmDialogComponent.OpenWait(
      this.dialog,
      `Delete ${this.itemTitle}?`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.delete(item);
    if ('id' in item) {
      this.itemsChange.emit(this.items.filter(value => value.id != item.id));
    }
    this.snackBar.open(`${this.itemTitle} Deleted`, null, {duration: 2000});
  }

  async onSave(item: T, panel: MatExpansionPanel, form: NgForm, isNew = false) {
    item = await this.save(item);
    this.snackBar.open(`${this.itemTitle} ${isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
    panel.close();
    if (isNew) {
      form.resetForm();
      this.itemsChange.emit([...this.items, item]);
    }
  }

  ngOnInit() {
    if (this.save == null) throw new Error('Save callback not provided');
    if (this.delete == null) throw new Error('Delete callback not provided');
  }
}
