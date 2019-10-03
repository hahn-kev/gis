import { JobStatusNamePipe } from './job-status-name.pipe';

describe('JobStatusNamePipe', () => {
  it('create an instance', () => {
    const pipe = new JobStatusNamePipe();
    expect(pipe).toBeTruthy();
  });
});
