using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;

namespace Backend.Services
{
    public class AttachmentService
    {
        private readonly AttachmentRepository _attachmentRepository;
        private readonly IEntityService _entityService;

        public AttachmentService(AttachmentRepository attachmentRepository, IEntityService entityService)
        {
            _attachmentRepository = attachmentRepository;
            _entityService = entityService;
        }

        public IList<Attachment> AttachmentsByAttachedId(Guid attachedId)
        {
            if (attachedId == Guid.Empty) return new List<Attachment>();
            return _attachmentRepository.Attachments.Where(attachment => attachment.AttachedToId == attachedId)
                .ToList();
        }

        public void Save(Attachment attachment)
        {
            _entityService.Save(attachment);
        }

        public void Delete(Guid id)
        {
            _entityService.Delete<Attachment>(id);
        }
    }
}