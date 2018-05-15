import { Component, EventEmitter, Input, OnInit, Output, QueryList, TemplateRef, ViewChildren } from '@angular/core';
import { AccordionListContentDirective } from './accordion-list-content.directive';
import { AccordionListFormDirective } from './accordion-list-form.directive';
import { MatExpansionPanel } from '@angular/material';
import { NgForm } from '@angular/forms';

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

  @Output() delete = new EventEmitter<T>();
  @Output() save = new EventEmitter<T>();
  @ViewChildren(MatExpansionPanel) expansionPanels: QueryList<MatExpansionPanel>;
  newForm: AccordionListFormDirective<T>;
  forms: AccordionListFormDirective<T>[] = [];
  public header: TemplateRef<{ $implicit: T, index: number }>;
  public content: AccordionListContentDirective<T>;

  constructor() {
  }

  onDelete(item: T) {
    this.delete.emit(item);
  }

  onSave(item: T, panel: MatExpansionPanel, form: NgForm, isNew = false) {
    this.save.emit(item);
    panel.close();
    if (isNew) form.resetForm();
  }

  ngOnInit() {
  }
}
