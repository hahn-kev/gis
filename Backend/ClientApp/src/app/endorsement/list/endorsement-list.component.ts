import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { UrlBindingService } from '../../services/url-binding.service';
import { ActivatedRoute } from '@angular/router';
import { AppDataSource } from '../../classes/app-data-source';
import { Endorsement } from '../endorsement';

@Component({
  selector: 'app-endorsement-list',
  templateUrl: './endorsement-list.component.html',
  styleUrls: ['./endorsement-list.component.scss'],
  providers: [UrlBindingService]
})
export class EndorsementListComponent implements OnInit {
  public dataSource: AppDataSource<Endorsement>;
  @ViewChild(MatSort, {static: true}) sort: MatSort;

  constructor(private route: ActivatedRoute, public urlBinding: UrlBindingService<{ search: string }>) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<Endorsement>();
    this.dataSource.bindToRouteData(this.route, 'endorsements');
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.dataSource.sort = this.sort;
    this.dataSource.filterPredicate = ((data, filter) =>
      data.name.toUpperCase().includes(filter));
    this.urlBinding.loadFromParams();
  }

}
