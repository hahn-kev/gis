import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { RoleWithJob } from '../../role';
import { EvaluationResult, EvaluationWithNames } from './evaluation';
import { Person } from '../../person';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-evaluation',
  templateUrl: './evaluation.component.html',
  styleUrls: ['./evaluation.component.scss']
})
export class EvaluationComponent implements OnInit {
  evaluationResults = Object.keys(EvaluationResult);
  @Input() readonly = false;
  @Input() evaluation: EvaluationWithNames;
  @Input() formId: string;
  @Input() roles: RoleWithJob[];
  @ViewChild(NgForm, {static: true}) form: NgForm;
  public people: Person[];

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.route.data.subscribe((value: {
      people: Person[]
    }) => {
      this.people = value.people.filter(person => person.id != this.evaluation.personId);
    });
  }

  updateJobTitle(roleId: string) {
    let role = this.roles.find(value => value.id == roleId);
    this.evaluation.jobTitle = role ? role.job.title : '';
  }

}
