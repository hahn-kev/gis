import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as moment from 'moment';

@Component({
  selector: 'app-sandbox',
  templateUrl: './sandbox.component.html',
  styleUrls: ['./sandbox.component.scss']
})
export class SandboxComponent implements OnInit {
  public date: moment.Moment;

  constructor(private http: HttpClient) {
  }

  ngOnInit() {
  }

  async test() {
    console.log('date:', this.date.toJSON());
    let val = await this.http.post<{ date: string }>('api/test', {date: this.date}).toPromise();
    console.log('date back:', moment(val.date).toISOString(true));
  }

}
