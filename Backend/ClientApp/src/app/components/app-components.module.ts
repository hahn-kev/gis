import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccordionListComponent } from './accordion-list/accordion-list.component';
import { AccordionListContentDirective } from './accordion-list/accordion-list-content.directive';
import { AccordionListCustomActionDirective } from './accordion-list/accordion-list-custom-action.directive';
import { AccordionListFormDirective } from './accordion-list/accordion-list-form.directive';
import { AccordionListHeaderDirective } from './accordion-list/accordion-list-header.directive';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatBadgeModule } from '@angular/material/badge';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatOptionModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTreeModule } from '@angular/material/tree';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
import { ObserversModule } from '@angular/cdk/observers';
import { FormsModule } from '@angular/forms';
import { IsUserPolicyPipe } from '../services/auth/is-user-policy.pipe';
import { AppTemplateContentDirective } from '../directives/app-template-content.directive';
import { ExportButtonComponent } from './export-button/export-button.component';
import { RenderTemplateDialogComponent } from './render-template-dialog/render-template-dialog.component';
import { TitleCasePipe } from '../services/title-case.pipe';
import { DialogDirective } from './render-template-dialog/dialog.directive';
import { QuickAddComponent } from '../people/person/quick-add/quick-add.component';
import { QuickAddDirective } from '../people/person/quick-add/quick-add.directive';
import { QuickAddButtonComponent } from '../people/person/quick-add/quick-add-button.component';
import { RoleComponent } from '../people/person/role.component';
import { CookieService } from 'ngx-cookie-service';
import { RouterModule } from '@angular/router';
import { ToolbarContentDirective } from '../toolbar/toolbar-content.directive';
import { ToolbarTemplateDirective } from '../toolbar/toolbar-template.directive';
import { FindFormPipe } from './accordion-list/find-form.pipe';
import { LeaveTypeNamePipe } from '../people/leave-request/leave-type-name.pipe';
import { PickFileDirective } from '../google-picker/pick-file.directive';

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
    FindFormPipe,
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
    ToolbarContentDirective,
    ToolbarTemplateDirective,
    LeaveTypeNamePipe,
    PickFileDirective
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
    ToolbarContentDirective,
    ToolbarTemplateDirective,
    LeaveTypeNamePipe,
    PickFileDirective,
    ...matModules,
  ],
  entryComponents: [
    RenderTemplateDialogComponent,
    QuickAddComponent,
  ]
})
export class AppComponentsModule {}
