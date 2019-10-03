import { Component, OnInit } from '@angular/core';
import { TrainingRequirement } from '../training-requirement';
import { AppDataSource } from '../../../classes/app-data-source';
import { ActivatedRoute } from '@angular/router';
import * as moment from 'moment';

@Component({
  selector: 'app-training-list',
  templateUrl: './training-list.component.html',
  styleUrls: ['./training-list.component.scss']
})
export class TrainingListComponent implements OnInit {
  public dataSource: AppDataSource<TrainingRequirement>;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<TrainingRequirement>();
    this.dataSource.bindToRouteData(this.route, 'trainingRequirements');
  }

  formatTime(months: number) {
    if (months == -1) return 'Only required once';
    return moment.duration(months, 'M').humanize();
  }
}
