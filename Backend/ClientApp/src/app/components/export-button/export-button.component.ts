import { Component, Input, OnInit } from '@angular/core';
import { CsvService } from '../../services/csv.service';
import { AppDataSource } from '../../classes/app-data-source';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-export-button',
  templateUrl: './export-button.component.html',
  styleUrls: ['./export-button.component.scss'],
  providers: [DatePipe]
})
export class ExportButtonComponent implements OnInit {
  @Input() values: any[] | AppDataSource<any>;
  @Input() fileName = 'csvData';

  @Input() columns: string[];


  constructor(private csvService: CsvService, private datePipe: DatePipe) {
  }

  ngOnInit() {
  }

  public canFilterByColumns() {
    return this.values instanceof AppDataSource && this.columns;
  }

  exportCsv(exportAll: boolean = false) {
    let valuesToExport: any[];
    if (exportAll || !this.canFilterByColumns()) {

      if ((<AppDataSource<any>>this.values).filteredData) {
        valuesToExport = (<AppDataSource<any>>this.values).filteredData;
      } else {
        valuesToExport = <any[]>this.values;
      }
    } else {
      valuesToExport = this.exportFromTable(<AppDataSource<any>>this.values, this.columns);
    }
    this.csvService.saveToCsv(valuesToExport, this.fileName);
  }

  exportFromTable(values: AppDataSource<any>, columns: string[]) {
    let results = [];
    for (let data of values.filteredData) {
      let result = {};
      for (let column of columns) {
        result[column] = values.sortingDataAccessor(data, column);
        if (column.toUpperCase().includes('DATE')) {
          result[column] = this.datePipe.transform(result[column]);
        }
      }
      results.push(result);
    }
    return results;
  }
}
