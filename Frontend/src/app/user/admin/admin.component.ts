import {Component, OnInit} from '@angular/core';
import {User} from '../user';
import {UserService} from '../user.service';
import {AppDataSource} from '../../classes/app-data-source';
import {UrlBindingService} from '../../services/url-binding.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss'],
  providers: [UrlBindingService]
})
export class AdminComponent implements OnInit {
  dataSource: AppDataSource<User>;

  constructor(private userService: UserService, private urlBinding: UrlBindingService<{ search: string }>) {
    this.dataSource = new AppDataSource<User>();
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.dataSource.filterPredicate = ((data, filter) =>
        (data.email || '').toUpperCase().startsWith(filter)
        || (data.personName || '').toUpperCase().startsWith(filter)
        || (data.userName || '').toUpperCase().startsWith(filter)
    );
  }

  ngOnInit() {
    this.dataSource.observe(this.userService.getUsers());
  }
}
