import { TestBed, inject } from '@angular/core/testing';

import { AttachmentService } from './attachment.service';

describe('AttachmentService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AttachmentService]
    });
  });

  it('should be created', inject([AttachmentService], (service: AttachmentService) => {
    expect(service).toBeTruthy();
  }));
});
