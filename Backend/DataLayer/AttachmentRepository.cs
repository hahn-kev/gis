using System.Linq;
using Backend.Entities;

namespace Backend.DataLayer
{
    public class AttachmentRepository
    {
        private IDbConnection _dbConnection;

        public AttachmentRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Attachment> Attachments => _dbConnection.Attachments;
    }
}