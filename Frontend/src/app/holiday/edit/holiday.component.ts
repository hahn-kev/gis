import { Component, OnInit } from '@angular/core';
import { BaseEditComponent } from '../../components/base-edit-component';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { MatDialog, MatSnackBar } from '@angular/material';
import { Holiday } from '../../people/leave-request/holiday';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { HolidayService } from '../holiday.service';

@Component({
  selector: 'app-holiday',
  templateUrl: './holiday.component.html',
  styleUrls: ['./holiday.component.scss']
})
export class HolidayComponent extends BaseEditComponent implements OnInit {
  public holiday: Holiday;
  public isNew: boolean;

  constructor(
    private holidayService: HolidayService,
    private route: ActivatedRoute,
    private location: Location,
    private snackBar: MatSnackBar,
    dialog: MatDialog) {
    super(dialog);

  }

  ngOnInit() {
    this.route.data.subscribe(value => {
      this.holiday = value.holiday;
      this.isNew = !this.holiday.id;
    });
  }

  async save() {
    this.holiday = await this.holidayService.saveHoliday(this.holiday).toPromise();
    this.location.back();
    this.snackBar.open(`${this.holiday.name} ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async deleteHoliday() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog, 'Delete Holiday?', 'Delete', 'Cancel');
    if (!result) return;
    await this.holidayService.deleteHoliday(this.holiday.id).toPromise();
    this.location.back();
    this.snackBar.open(`${this.holiday.name} Deleted`, null, {duration: 2000});
  }

}
