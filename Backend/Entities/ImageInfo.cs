using LinqToDB.Mapping;

namespace Backend.Entities
{
    [Table("ImageInfo", IsColumnAttributeRequired = false)]
    public class ImageInfo
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        [Column(DbType = "oid")]
        public uint ImageId { get; set; }
    }
}