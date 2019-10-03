import { Injectable } from '@angular/core';
import * as FileSaver from 'file-saver';
import * as json2csv from 'json2csv';

@Injectable()
export class CsvService {

  constructor() {
  }

  toCsv<T>(values: T[]) {
    if (!values || values.length == 0) throw new Error('Error list of values is empty');
    //docs for json2csv https://github.com/zemirco/json2csv
    return json2csv.parse(values, {flatten: true});
  }

  toCsvBlob<T>(values: T[]) {
    return new Blob([this.toCsv(values)], {type: 'text/csv'});
  }

  saveToCsv<T>(values: T[], defaultFilename: string) {
    let csvBlob = this.toCsvBlob(values);
    FileSaver.saveAs(csvBlob, defaultFilename + '.csv');
  }

}
