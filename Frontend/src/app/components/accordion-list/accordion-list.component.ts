import { Component, ElementRef, EventEmitter, Input, OnInit, Output, QueryList, ViewChildren } from '@angular/core';
import { AccordionListContentDirective } from './accordion-list-content.directive';
import { AccordionListFormDirective } from './accordion-list-form.directive';
import { MatDialog, MatExpansionPanel, MatSnackBar } from '@angular/material';
import { NgForm } from '@angular/forms';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { BaseEntity } from '../../classes/base-entity';
import { AccordionListHeaderDirective } from './accordion-list-header.directive';
import { Observable } from 'rxjs';
import { AccordionListCustomActionDirective } from './accordion-list-custom-action.directive';

@Component({
  selector: 'app-accordion-list',
  templateUrl: './accordion-list.component.html',
  styleUrls: ['./accordion-list.component.scss']
})
export class AccordionListComponent<T extends BaseEntity> implements OnInit {
  @Input() caption: string = null;
  @Input() createNewItem: () => T;
  newItem: T;
  @Input() itemTitle = 'Item';
  @Input() addNew = true;
  @Input() showActions = true;
  @Input() items: T[] = [];
  @Output() itemsChange = new EventEmitter<T[]>();

  @Input() save: (item: T) => Promise<T> | Observable<T>;
  @Input() delete: (item: T) => Promise<boolean> | Observable<boolean>;
  @ViewChildren(MatExpansionPanel) expansionPanels: QueryList<MatExpansionPanel>;
  @ViewChildren(MatExpansionPanel, {read: ElementRef}) expansionPanelElements: QueryList<ElementRef<HTMLElement>>;
  public header: AccordionListHeaderDirective<T>;
  public content: AccordionListContentDirective<T>;
  public customAction: AccordionListCustomActionDirective<T>;
  private forms: AccordionListFormDirective<T>[] = [];

  constructor(private snackBar: MatSnackBar, private dialog: MatDialog, private element: ElementRef<HTMLElement>) {
  }

  async onDelete(item: T) {
    let result = await ConfirmDialogComponent.OpenWait(
      this.dialog,
      `Delete ${this.itemTitle}?`,
      'Delete',
      'Cancel');
    if (!result) return;
    let deleteResult = this.delete(item);
    if ('toPromise' in deleteResult) deleteResult = deleteResult.toPromise();
    await deleteResult;
    if ('id' in item) {
      this.itemsChange.emit(this.items.filter(value => value.id != item.id));
    }
    this.snackBar.open(`${this.itemTitle} Deleted`, null, {duration: 2000});
  }

  async onSave(item: T, panel: MatExpansionPanel, form: NgForm, isNew = false) {
    let saveResult = this.save(item);
    if ('toPromise' in saveResult) saveResult = saveResult.toPromise();
    item = await saveResult;
    this.snackBar.open(`${this.itemTitle} ${isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
    panel.close();
    if (isNew) {
      this.itemsChange.emit([...this.items, item]);
      this.newItem = this.createNewItem();
      form.resetForm();
    }
  }

  ngOnInit() {
    if (this.save == null) throw new Error('Save callback not provided');
    if (this.delete == null) throw new Error('Delete callback not provided');
    if (this.header == null) throw new Error('Missing *appAccordionListHeader');
    if (this.content == null) throw new Error('Missing *appAccordionListContent');
  }

  findIndex(form: AccordionListFormDirective<T>) {
    let foundIndex = -1;
    this.expansionPanelElements.forEach((item, index) => {
      if (foundIndex != -1) return;
      if (form.hasAsParent(item.nativeElement)) {
        foundIndex = index;
      }
    });
    return foundIndex - 1;
  }

  addForm(form: AccordionListFormDirective<T>) {
    this.forms = [...this.forms, form];
  }

  removeForm(form: AccordionListFormDirective<T>) {
    this.forms = [...this.forms.filter(value => value == form)];
  }

  findForm(index: number) {
    for (let form of this.forms) {
      if (form.getContext().index == index) return form;
    }
    return null;
  }

  matchesElement(element: HTMLElement) {
    return this.element.nativeElement == element;
  }
}
