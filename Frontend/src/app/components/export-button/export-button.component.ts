import { Component, Input, OnInit } from '@angular/core';
import { CsvService } from '../../services/csv.service';
import { AppDataSource } from '../../classes/app-data-source';

@Component({
  selector: 'app-export-button',
  templateUrl: './export-button.component.html',
  styleUrls: ['./export-button.component.scss']
})
export class ExportButtonComponent implements OnInit {
  @Input() values: any[] | AppDataSource<any>;
  @Input() fileName = 'csvData';

  constructor(private csvService: CsvService) {
  }

  ngOnInit() {
  }

  exportCsv() {
    let valuesToExport: any[];
    if ((<AppDataSource<any>>this.values).filteredData) {
      valuesToExport = (<AppDataSource<any>>this.values).filteredData;
    } else {
      valuesToExport = <any[]> this.values;
    }
    this.csvService.saveToCsv(valuesToExport, this.fileName);
  }
}
