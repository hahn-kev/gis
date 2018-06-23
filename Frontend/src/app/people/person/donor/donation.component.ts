import { Component, Input, OnInit } from '@angular/core';
import { PersonWithOthers } from '../../person';
import { Donation } from '../../donor';
import { MissionOrgService } from '../../../mission-org/mission-org.service';
import { LazyLoadService } from '../../../services/lazy-load.service';
import { Observable } from 'rxjs';
import { MissionOrgWithNames } from '../../../mission-org/mission-org';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-donation',
  templateUrl: './donation.component.html',
  styleUrls: ['./donation.component.scss']
})
export class DonationComponent implements OnInit {
  @Input()
  public person: PersonWithOthers;

  @Input()
  public readonly: boolean;

  createDonation = () => {
    let donation = new Donation();
    donation.personId = this.person.id;
    if (this.person.donations.length > 0)
      donation.missionOrgId = this.person.donations[this.person.donations.length - 1].missionOrgId;
    return donation;
  };

  save = (donation: Donation) => {
    return this.http.post<Donation>('/api/donation', donation);
  };

  deleteDonation = (donation: Donation) => {
    return this.http.delete('/api/donation/' + donation.id, {responseType: 'text'});
  };
  public missionOrgs: Observable<MissionOrgWithNames[]>;

  constructor(missionOrgService: MissionOrgService, lazyLoad: LazyLoadService, private http: HttpClient) {
    this.missionOrgs = lazyLoad.share('missionOrgs', () => missionOrgService.list());
  }

  ngOnInit() {
  }

}
