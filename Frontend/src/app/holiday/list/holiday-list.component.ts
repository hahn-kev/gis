import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { MatSort } from '@angular/material';
import { ActivatedRoute } from '@angular/router';
import { UrlBindingService } from '../../services/url-binding.service';
import { Holiday } from '../../people/leave-request/holiday';

@Component({
  selector: 'app-holiday-list',
  templateUrl: './holiday-list.component.html',
  styleUrls: ['./holiday-list.component.scss'],
  providers: [UrlBindingService]
})
export class HolidayListComponent implements OnInit {
  public dataSource: AppDataSource<Holiday>;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute, public urlBinding: UrlBindingService<{ search: string }>) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<Holiday>();
    this.dataSource.bindToRouteData(this.route, 'holidays');
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.dataSource.sort = this.sort;
    this.dataSource.filterPredicate = ((data, filter) =>
      data.name.toUpperCase().startsWith(filter));
    this.urlBinding.loadFromParams();
  }

}
