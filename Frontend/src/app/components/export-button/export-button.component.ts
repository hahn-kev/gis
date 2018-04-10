import { Component, Input, OnInit } from '@angular/core';
import { CsvService } from '../../services/csv.service';

@Component({
  selector: 'app-export-button',
  templateUrl: './export-button.component.html',
  styleUrls: ['./export-button.component.scss']
})
export class ExportButtonComponent implements OnInit {
  @Input() values: any[];
  @Input() fileName = 'csvData';

  constructor(private csvService: CsvService) {
  }

  ngOnInit() {
  }

  exportCsv() {
    this.csvService.saveToCsv(this.values, this.fileName);
  }
}
