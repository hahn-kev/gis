import { Component, EventEmitter, Input, OnInit, Output, TemplateRef } from '@angular/core';
import { AccordionListContentDirective } from './accordion-list-content.directive';
import { AccordionListFormDirective } from './accordion-list-form.directive';

@Component({
  selector: 'app-accordion-list',
  templateUrl: './accordion-list.component.html',
  styleUrls: ['./accordion-list.component.scss']
})
export class AccordionListComponent<T> implements OnInit {
  @Input() createNewItem: () => T;
  newItem: T;
  @Input() itemTitle = 'Item';
  @Input() addNew = true;
  @Input() showActions = true;
  @Input() items: T[] = [];

  @Output() delete = new EventEmitter<T>();
  newForm: AccordionListFormDirective<T>;
  forms: AccordionListFormDirective<T>[] = [];
  public header: TemplateRef<{ $implicit: T, index: number }>;
  public content: AccordionListContentDirective<T>;

  constructor() {
  }

  onDelete(item: T) {
    this.delete.emit(item);
  }

  ngOnInit() {
  }
}
