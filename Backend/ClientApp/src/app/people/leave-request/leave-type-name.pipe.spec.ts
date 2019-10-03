import { LeaveTypeNamePipe } from './leave-type-name.pipe';

describe('LeaveTypeNamePipe', () => {
  it('create an instance', () => {
    const pipe = new LeaveTypeNamePipe();
    expect(pipe).toBeTruthy();
  });
});
