import { inject, TestBed } from '@angular/core/testing';

import { AttachmentService } from './attachment.service';

xdescribe('AttachmentService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AttachmentService]
    });
  });

  it('should be created', inject([AttachmentService], (service: AttachmentService) => {
    expect(service).toBeTruthy();
  }));
});
