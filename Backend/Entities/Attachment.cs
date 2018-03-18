using System;

namespace Backend.Entities
{
    public class Attachment : BaseEntity
    {
        public string Name { get; set; }
        public string FileType { get; set; }
        public string GoogleId { get; set; }
        public string DownloadUrl { get; set; }
        public Guid AttachedToId { get; set; }
    }
}