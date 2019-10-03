import { Component, Input, OnInit } from '@angular/core';
import { Donor, DonorStatus } from '../../donor';

@Component({
  selector: 'app-donor',
  templateUrl: './donor.component.html',
  styleUrls: ['./donor.component.scss']
})
export class DonorComponent implements OnInit {
  readonly donorStatusList = Object.keys(DonorStatus);
  @Input() donor: Donor;
  @Input() readonly: boolean;

  constructor() {
  }

  ngOnInit() {
  }

}
