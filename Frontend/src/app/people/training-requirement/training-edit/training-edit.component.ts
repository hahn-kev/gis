import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TrainingRequirementService } from '../training-requirement.service';
import { TrainingRequirement, TrainingScope } from '../training-requirement';
import { Year } from '../year';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ConfirmDialogComponent } from '../../../dialog/confirm-dialog/confirm-dialog.component';
import { BaseEditComponent } from '../../../components/base-edit-component';
import { Person } from '../../person';
import { OrgGroup } from '../../groups/org-group';

@Component({
  selector: 'app-training-edit',
  templateUrl: './training-edit.component.html',
  styleUrls: ['./training-edit.component.scss']
})
export class TrainingEditComponent extends BaseEditComponent implements OnInit {
  public training: TrainingRequirement;
  public years: Year[];
  public people: Person[];
  public groups: OrgGroup[];
  public isNew = false;

  constructor(private route: ActivatedRoute,
              private trainingService: TrainingRequirementService,
              private router: Router,
              dialog: MatDialog,
              private snackBar: MatSnackBar) {
    super(dialog);
  }

  ngOnInit(): void {
    this.route.data.subscribe((value: {
      training: TrainingRequirement
      people: Person[],
      groups: OrgGroup[]
    }) => {
      this.groups = value.groups;
      this.people = value.people;
      this.training = value.training;
      this.isNew = !this.training.id;
    });
    this.years = Year.years();
  }

  get for() {
    if (this.training.scope !== TrainingScope.Department) return this.training.scope;
    return this.training.departmentId;
  }

  set for(value: string | TrainingScope) {
    if (value == TrainingScope.AllStaff) {
      this.training.scope = value;
    } else {
      this.training.scope = TrainingScope.Department;
      this.training.departmentId = value;
    }
  }

  async save(): Promise<void> {
    await this.trainingService.save(this.training);
    this.router.navigate(['training', 'list']);
    this.snackBar.open(`Training Requirement ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async delete() {
    const result = await ConfirmDialogComponent.OpenWait(this.dialog,
      `Deleting Training requirement, any training records will also be deleted`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.trainingService.deleteRequirement(this.training.id);
    this.router.navigate(['training', 'list'], {relativeTo: this.route});
    this.snackBar.open(`Training Requirement Deleted`, null, {duration: 2000});
  }

}
