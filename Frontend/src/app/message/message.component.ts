import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-message',
  templateUrl: './message.component.html',
  styleUrls: ['./message.component.scss']
})
export class MessageComponent implements OnInit {
  public message: string;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.message = this.route.snapshot.queryParams['text'];
  }
}
