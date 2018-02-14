import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Self } from './self';

@Component({
  selector: 'app-self',
  templateUrl: './self.component.html',
  styleUrls: ['./self.component.scss']
})
export class SelfComponent implements OnInit {
  public self: Self;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.route.data.subscribe((value: { self: Self }) => this.self = value.self);
  }

}
