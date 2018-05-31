import { Component, OnInit } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { PersonAndLeaveDetails } from '../person-and-leave-details';
import { ActivatedRoute } from '@angular/router';
import { UrlBindingService } from '../../../services/url-binding.service';
import { PersonLeaveModel } from './person-leave-model';

@Component({
  selector: 'app-leave-report',
  templateUrl: './leave-report.component.html',
  styleUrls: ['./leave-report.component.scss'],
  providers: [UrlBindingService]
})
export class LeaveReportComponent implements OnInit {
  public dataSource = new AppDataSource<PersonLeaveModel>();

  constructor(private route: ActivatedRoute, public urlBinding: UrlBindingService<{ search: string }>) {
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.trim().toUpperCase());
    this.urlBinding.loadFromParams();
    this.dataSource.filterPredicate = ((data, filter) =>
      data.person.firstName.toUpperCase().startsWith(filter)
      || data.person.lastName.toUpperCase().startsWith(filter)
      || data.person.preferredName.toUpperCase().startsWith(filter)
    );
    this.route.data.subscribe((value: { people: PersonAndLeaveDetails[] }) => {
      this.dataSource.data = value.people.map(p => {
        let plm = new PersonLeaveModel();
        plm.person = p.person;

        for (let leave of p.leaveUsages) plm.appendLeave(leave);
        return plm;
      });
    });
  }

  ngOnInit() {
  }
}

