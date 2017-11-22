import { Component, OnInit } from '@angular/core';
import { animate, state, style, transition, trigger } from '@angular/animations';

@Component({
  selector: 'app-flip-card',
  templateUrl: './flip-card.component.html',
  styleUrls: ['./flip-card.component.scss'],

  animations: [
    trigger('flipState', [
      state('active', style({
        transform: 'rotateY(180deg)'
      })),
      state('inactive', style({
        transform: 'rotateY(0)'
      })),
      transition('active => inactive', animate('500ms ease-out')),
      transition('inactive => active', animate('500ms ease-in'))
    ])
  ]
})
export class FlipCardComponent implements OnInit {

  public trigger: string = 'inactive';

  constructor() {
  }

  ngOnInit() {
  }

}
