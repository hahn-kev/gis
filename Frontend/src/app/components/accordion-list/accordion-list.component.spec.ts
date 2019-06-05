import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { AccordionListComponent } from './accordion-list.component';
import { BaseEntity } from '../../classes/base-entity';
import { MatDialog, MatExpansionModule, MatExpansionPanel, MatSnackBar } from '@angular/material';
import { Component, DebugElement, EventEmitter, Type, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { AccordionListHeaderDirective } from './accordion-list-header.directive';
import { AccordionListContentDirective } from './accordion-list-content.directive';
import { AccordionListFormDirective } from './accordion-list-form.directive';
import { AccordionListCustomActionDirective } from './accordion-list-custom-action.directive';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { FindFormPipe } from './find-form.pipe';
import Spy = jasmine.Spy;
import SpyObj = jasmine.SpyObj;

class BaseHostComponent<T> {
  public headerText = 'this is the header';
  public contentText = 'this is the content';

  @ViewChild(AccordionListComponent, {static: false}) listComponent: AccordionListComponent<BaseEntity>;
  public caption = 'test caption';
  public createNewItem: () => T = jasmine.createSpy('createNewSpy', () => new BaseEntity()).and.callThrough();
  public itemTitle = 'Item';
  public addNew = true;
  public showActions = true;
  public items: T[] = [];
  public itemsChange = new EventEmitter<T[]>();
  public save: (item: T) => Promise<T> | Observable<T> = jasmine.createSpy('saveSpy');
  public delete: (item: T) => Promise<boolean> | Observable<boolean> = jasmine.createSpy('deleteSpy');
}

@Component({
  template: `
    <app-accordion-list
      [caption]="caption"
      [createNewItem]="createNewItem"
      [itemTitle]="itemTitle"
      [addNew]="addNew"
      [showActions]="showActions"
      [items]="items"
      (itemsChange)="itemsChange.emit($event)"
      [save]="save"
      [delete]="delete">
      <ng-container *appAccordionListContent>{{contentText}}</ng-container>
      <ng-container *appAccordionListHeader>{{headerText}}</ng-container>
    </app-accordion-list>
  `
})
// tslint:disable-next-line:component-class-suffix
class HostComponentSimple<T> extends BaseHostComponent<T> {}

@Component({
  template: `
    <app-accordion-list
      [caption]="caption"
      [createNewItem]="createNewItem"
      [itemTitle]="itemTitle"
      [addNew]="addNew"
      [showActions]="showActions"
      [items]="items"
      (itemsChange)="itemsChange.emit($event)"
      [save]="save"
      [delete]="delete">
      <ng-container *appAccordionListHeader="let item;">{{item.id}}</ng-container>
      <ng-container *appAccordionListContent="let item;">
        <form appAccordionListForm>
          <input id="input1" required name="input1" [(ngModel)]="item.id">
        </form>
      </ng-container>
    </app-accordion-list>
  `
})
// tslint:disable-next-line:component-class-suffix
class HostComponentForm<T> extends BaseHostComponent<T> {}


describe('AccordionListComponent', () => {
  let hostComponent: BaseHostComponent<BaseEntity>;
  let fixture: ComponentFixture<BaseHostComponent<BaseEntity>>;
  let saveSpy: Spy;
  let deleteSpy: Spy;
  let itemsChangesSpy: Spy;

  let snackbarSpy: SpyObj<MatSnackBar>;
  let dialogSpy: SpyObj<MatDialog>;

  function getExpansionPanels() {
    return fixture.debugElement.queryAll(value => value.name == 'mat-expansion-panel');
  }

  function getNewPanel() {
    let panels = getExpansionPanels();
    expect(panels[0]).toBeTruthy();
    return panels[0];
  }

  function panelInstance(panel: DebugElement): MatExpansionPanel {
    return panel.componentInstance;
  }

  function expandPanel(panel: DebugElement | number) {
    if (typeof panel == 'number') panel = getExpansionPanels()[panel];
    panelInstance(panel).open();
    return doubleCheck();
  }

  function doubleCheck() {
    fixture.detectChanges();
    return fixture.whenStable().then(() => {
      fixture.detectChanges();
    });
  }

  function createComponent<T extends BaseHostComponent<BaseEntity>>(t: Type<T>) {
    fixture = <ComponentFixture<BaseHostComponent<BaseEntity>>>TestBed.createComponent(t);
    hostComponent = fixture.componentInstance;
    saveSpy = <Spy>hostComponent.save;
    saveSpy.and.callFake((v) => {
      return Promise.resolve(v);
    });

    deleteSpy = <Spy>hostComponent.delete;
    deleteSpy.and.callFake((v) => {
      return Promise.resolve();
    });
    itemsChangesSpy = jasmine.createSpy('itemsChangeSpy');
    hostComponent.itemsChange.subscribe(items => itemsChangesSpy(items));
    snackbarSpy = TestBed.get(MatSnackBar);
    dialogSpy = TestBed.get(MatDialog);

    fixture.detectChanges();
    fixture.whenStable().then(() => {
      fixture.detectChanges();
    });
  }

  beforeEach(async(() => {
    return TestBed.configureTestingModule({
      imports: [MatExpansionModule, NoopAnimationsModule, FormsModule],
      declarations: [
        HostComponentSimple,
        HostComponentForm,
        AccordionListComponent,
        AccordionListHeaderDirective,
        AccordionListContentDirective,
        AccordionListFormDirective,
        AccordionListCustomActionDirective,
        FindFormPipe
      ],
      providers: [
        {provide: MatSnackBar, useValue: jasmine.createSpyObj('MatSnackBar', ['open'])},
        {provide: MatDialog, useValue: jasmine.createSpyObj('MatDialog', ['open'])},
      ]
    }).compileComponents();
  }));

  describe('simple', () => {
    beforeEach(() => createComponent(HostComponentSimple));

    it('should create', () => {
      expect(hostComponent).toBeTruthy();
    });

    it('should have an add new expansion panel', () => {
      let nodes = getExpansionPanels();
      expect(nodes.length).toBe(1);
      let newPanel = nodes[0];
      expect(newPanel.nativeElement.textContent)
        .toContain('Add new');
    });

    it('should render some panels', () => {
      hostComponent.items = [new BaseEntity()];
      fixture.detectChanges();
      let nodes = getExpansionPanels();
      expect(nodes.length).toBe(2);
      let dataPanel = nodes[1];
      expect(dataPanel.nativeElement.textContent)
        .toContain(hostComponent.headerText);
    });
  });

  describe('form', () => {
    beforeEach(() => createComponent(HostComponentForm));

    function expectDisabled(element: DebugElement) {
      expect(element.properties.disabled).toBeTruthy();
    }

    function expectNotDisabled(element: DebugElement) {
      expect(element.properties.disabled).toBeFalsy();
    }

    function getSaveButton(panel: DebugElement) {
      return panel.query(value => (value.attributes.id || value.properties.id || '').includes('save-'));
    }

    function clickSave(panel: DebugElement | number) {
      if (typeof panel == 'number') panel = getExpansionPanels()[panel];
      let saveButton = getSaveButton(panel);
      saveButton.nativeElement.click();
      fixture.detectChanges();
    }

    describe('new', () => {
      let newPanel: DebugElement;
      beforeEach(async(async () => {
        newPanel = getNewPanel();
        await expandPanel(newPanel);
      }));
      describe('save button', () => {
        it('should disable the save button if validation failed', () => {
          let saveButton = getSaveButton(newPanel);
          expectDisabled(saveButton);
        });
        it('should enable the save button if validation passed', async(async () => {
          hostComponent.listComponent.newItem.id = 'newId';
          await doubleCheck();
          let saveButton = getSaveButton(newPanel);
          expectNotDisabled(saveButton);
        }));
      });
      describe('save', () => {
        let expectedNewItem: BaseEntity;
        beforeEach(async(async () => {
          hostComponent.listComponent.newItem.id = 'newId';
          expectedNewItem = hostComponent.listComponent.newItem;

          await doubleCheck();
          clickSave(newPanel);
          await doubleCheck();
        }));

        it('should call save with new', () => {
          expect(saveSpy).toHaveBeenCalledWith(expectedNewItem);
        });

        it('should emit the new items list', () => {
          expect(itemsChangesSpy).toHaveBeenCalledTimes(1);
          let values: BaseEntity[] = itemsChangesSpy.calls.first().args[0];
          expect(values.length).toBe(1);
          expect(values[0].id).toBe(expectedNewItem.id);
        });

        it('should open a snack bar', () => {
          expect(snackbarSpy.open).toHaveBeenCalled();
        });
      });


    });

    describe('list', () => {
      let baseEntity1: BaseEntity;
      let baseEntity2: BaseEntity;
      beforeEach(() => {
        baseEntity1 = new BaseEntity();
        baseEntity1.id = '1';
        baseEntity2 = new BaseEntity();
        baseEntity2.id = '2';
        hostComponent.items = [baseEntity1, baseEntity2];
        fixture.detectChanges();
      });

      function expectPanelToContainText(panelIndex: number, text: string) {
        fixture.detectChanges();
        let nodes = getExpansionPanels();
        expect(nodes.length).toBe(3);
        let dataPanel = nodes[panelIndex];
        expect(dataPanel.nativeElement.textContent)
          .toContain(text);
      }

      it('should update on sorted', () => {
        expectPanelToContainText(1, baseEntity1.id);
        hostComponent.items = [baseEntity2, baseEntity1];
        expectPanelToContainText(1, baseEntity2.id);
      });

      it('should save the proper entity', async(async () => {
        await expandPanel(1);
        clickSave(1);
        expect(saveSpy).toHaveBeenCalledWith(baseEntity1);
      }));

      it('should submit the expected entity on save after sorted', async(async () => {
        await expandPanel(1);
        await expandPanel(2);

        hostComponent.items = [baseEntity2, baseEntity1];
        expectPanelToContainText(1, baseEntity2.id);
        clickSave(1);
        await doubleCheck();
        expect(saveSpy).toHaveBeenCalledWith(baseEntity2);
      }));
    });
  });


});
