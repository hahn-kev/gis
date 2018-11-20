import { MyDatePipe } from './my-date.pipe';

describe('MyDatePipe', () => {
  it('create an instance', () => {
    const pipe = new MyDatePipe('en');
    expect(pipe).toBeTruthy();
  });
});
