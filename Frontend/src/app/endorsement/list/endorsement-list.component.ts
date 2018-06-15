import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSort } from '@angular/material';
import { UrlBindingService } from '../../services/url-binding.service';
import { ActivatedRoute } from '@angular/router';
import { AppDataSource } from '../../classes/app-data-source';
import { Person } from '../../people/person';

@Component({
  selector: 'app-endorsement-list',
  templateUrl: './endorsement-list.component.html',
  styleUrls: ['./endorsement-list.component.scss'],
  providers: [UrlBindingService]
})
export class EndorsementListComponent implements OnInit {
  public dataSource: AppDataSource<Person>;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute, public urlBinding: UrlBindingService<{ search: string }>) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<Person>();
    this.dataSource.bindToRouteData(this.route, 'endorsements');
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.dataSource.sort = this.sort;
    this.dataSource.filterPredicate = ((data, filter) =>
      data.firstName.toUpperCase().startsWith(filter) || data.lastName.toUpperCase().startsWith(filter));
    this.urlBinding.loadFromParams();
  }

}
