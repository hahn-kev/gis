import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { GradeService } from '../grade.service';
import { ConfirmDialogComponent } from '../../../dialog/confirm-dialog/confirm-dialog.component';
import { Grade } from '../grade';
import { BaseEditComponent } from '../../../components/base-edit-component';

@Component({
  selector: 'app-grade',
  templateUrl: './grade.component.html',
  styleUrls: ['./grade.component.scss']
})
export class GradeComponent extends BaseEditComponent implements OnInit {
  public grade: Grade;
  public isNew: boolean;

  constructor(private gradeService: GradeService,
              private route: ActivatedRoute,
              private router: Router,
              private snackBar: MatSnackBar,
              dialog: MatDialog) {
    super(dialog);
  }

  ngOnInit() {
    this.route.data.subscribe((value) => {
      this.grade = value.grade;
      this.isNew = !this.grade.id;
    });
  }

  async save() {
    await this.gradeService.save(this.grade);
    this.router.navigate(['/job/grade/list']);
    this.snackBar.open(`Grade ${this.grade.gradeNo} ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async deleteGrade() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog, 'Delete Grade?', 'Delete', 'Cancel');
    if (!result) return;
    await this.gradeService.delete(this.grade.id).toPromise();
    this.router.navigate(['/job/grade/list']);
    this.snackBar.open(`Grade ${this.grade.gradeNo} Deleted`, null, {duration: 2000});
  }
}
