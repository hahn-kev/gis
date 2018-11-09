import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {AccordionListComponent} from './accordion-list/accordion-list.component';
import {AccordionListContentDirective} from './accordion-list/accordion-list-content.directive';
import {AccordionListCustomActionDirective} from './accordion-list/accordion-list-custom-action.directive';
import {AccordionListFormDirective} from './accordion-list/accordion-list-form.directive';
import {AccordionListHeaderDirective} from './accordion-list/accordion-list-header.directive';
import {
  MatAutocompleteModule,
  MatBadgeModule,
  MatBottomSheetModule,
  MatButtonModule,
  MatButtonToggleModule,
  MatCardModule,
  MatCheckboxModule,
  MatChipsModule,
  MatDatepickerModule,
  MatDialogModule,
  MatExpansionModule,
  MatGridListModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatOptionModule,
  MatProgressBarModule,
  MatSelectModule,
  MatSidenavModule,
  MatSlideToggleModule,
  MatSnackBarModule,
  MatSortModule,
  MatTableModule,
  MatToolbarModule,
  MatTooltipModule,
  MatTreeModule
} from '@angular/material';
import {MatMomentDateModule} from '@angular/material-moment-adapter';
import {ObserversModule} from '@angular/cdk/observers';
import {FormsModule} from '@angular/forms';
import {IsUserPolicyPipe} from '../services/auth/is-user-policy.pipe';
import {AppTemplateContentDirective} from '../directives/app-template-content.directive';
import {ExportButtonComponent} from './export-button/export-button.component';
import {RenderTemplateDialogComponent} from './render-template-dialog/render-template-dialog.component';
import {TitleCasePipe} from '../services/title-case.pipe';
import {DialogDirective} from './render-template-dialog/dialog.directive';
import {QuickAddComponent} from '../people/person/quick-add/quick-add.component';
import {QuickAddDirective} from '../people/person/quick-add/quick-add.directive';
import {QuickAddButtonComponent} from '../people/person/quick-add/quick-add-button.component';
import {RoleComponent} from '../people/person/role.component';
import {CookieService} from 'ngx-cookie-service';
import {RouterModule} from '@angular/router';

const matModules = [
  MatButtonModule,
  MatInputModule,
  MatOptionModule,
  MatSelectModule,
  MatSidenavModule,
  MatToolbarModule,
  MatTooltipModule,
  MatCardModule,
  MatListModule,
  MatDialogModule,
  MatDatepickerModule,
  MatMomentDateModule,
  MatTableModule,
  MatSortModule,
  MatIconModule,
  MatCheckboxModule,
  MatSnackBarModule,
  MatProgressBarModule,
  MatTableModule,
  ObserversModule,
  MatButtonToggleModule,
  MatChipsModule,
  MatMenuModule,
  MatSlideToggleModule,
  MatExpansionModule,
  MatAutocompleteModule,
  MatGridListModule,
  MatBottomSheetModule,
  MatTreeModule,
  MatBadgeModule,
];

@NgModule({
  declarations: [
    AccordionListComponent,
    AccordionListContentDirective,
    AccordionListCustomActionDirective,
    AccordionListFormDirective,
    AccordionListHeaderDirective,
    AccordionListCustomActionDirective,
    IsUserPolicyPipe,
    AppTemplateContentDirective,
    ExportButtonComponent,
    RenderTemplateDialogComponent,
    TitleCasePipe,
    DialogDirective,
    QuickAddComponent,
    QuickAddDirective,
    QuickAddButtonComponent,
    RoleComponent,
  ],
  providers: [CookieService],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,

    ...matModules
  ],
  exports: [
    FormsModule,
    AccordionListComponent,
    AccordionListContentDirective,
    AccordionListCustomActionDirective,
    AccordionListFormDirective,
    AccordionListHeaderDirective,
    AccordionListCustomActionDirective,
    IsUserPolicyPipe,
    AppTemplateContentDirective,
    ExportButtonComponent,
    TitleCasePipe,
    DialogDirective,
    QuickAddDirective,
    QuickAddButtonComponent,
    RoleComponent,
    ...matModules
  ],
  entryComponents: [
    RenderTemplateDialogComponent,
    QuickAddComponent,
  ]
})
export class AppComponentsModule {}
