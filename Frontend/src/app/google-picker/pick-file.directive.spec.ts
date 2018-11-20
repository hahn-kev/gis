import { PickFileDirective } from './pick-file.directive';

describe('PickFileDirective', () => {
  it('should create an instance', () => {
    const directive = new PickFileDirective(jasmine.createSpyObj('drive service', ['openPicker']));
    expect(directive).toBeTruthy();
  });
});
