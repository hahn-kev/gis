import { Component, Inject, OnInit } from '@angular/core';
import { PersonService } from '../../person.service';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { PersonWithStaff } from '../../person';

@Component({
  selector: 'app-quick-add',
  templateUrl: './quick-add.component.html',
  styleUrls: ['./quick-add.component.scss']
})
export class QuickAddComponent implements OnInit {
  public person = new PersonWithStaff();

  constructor(private personService: PersonService,
              @Inject(MAT_DIALOG_DATA) private data: any,
              private dialogRef: MatDialogRef<QuickAddComponent>) {
  }

  ngOnInit() {
  }

  cancel() {
    this.dialogRef.close();
  }

  async add() {
    this.person = await this.personService.updatePerson(this.person);
    this.dialogRef.close(this.person);
  }
}
