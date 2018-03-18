// noinspection ES6UnusedImports
import {} from 'google.picker';

export type PickerDocument = typeof google.picker.Document;

export class PickerResponse {
  private data: any;

  constructor(data: any) {
    this.data = data;
  }

  get documents(): Array<PickerDocument> {
    let docs = this.data[google.picker.Response.DOCUMENTS];
    return docs.map(doc => {
      let newDoc = Object.assign({}, google.picker.Document);
      for (let propName of Object.keys(google.picker.Document)) {
        newDoc[propName] = doc[google.picker.Document[propName]];
      }
      return newDoc;
    });
  }

  get parents() {
    return this.data[google.picker.Response.PARENTS];
  }

  get view() {
    return this.data[google.picker.Response.VIEW];

  }
}
