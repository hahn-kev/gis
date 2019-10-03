import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppDataSource } from '../../../classes/app-data-source';
import { Grade } from '../grade';

@Component({
  selector: 'app-grade-list',
  templateUrl: './grade-list.component.html',
  styleUrls: ['./grade-list.component.scss']
})
export class GradeListComponent implements OnInit {
  public dataSource: AppDataSource<Grade>;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<Grade>();
    this.dataSource.bindToRouteData(this.route, 'grades');
  }

}
