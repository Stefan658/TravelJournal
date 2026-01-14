using NLog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;

using TravelJournal.Services.Interfaces;

namespace TravelJournal.Services.Implementations
{
    public class PhotoService : IPhotoService
    {
        private readonly IPhotoAccessor _photoAccessor;
        private readonly IUserAccessor _userAccessor;
        private readonly ISubscriptionService _subs;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PhotoService(
            IPhotoAccessor photoAccessor,
            IUserAccessor userAccessor,
            ISubscriptionService subs)
        {
            _photoAccessor = photoAccessor;
            _userAccessor = userAccessor;
            _subs = subs;
        }

        public IEnumerable<Photo> GetByEntry(int entryId)
            => _photoAccessor.GetByEntry(entryId);

        public void Upload(Photo photo, int userId)
        {
            var user = _userAccessor.GetById(userId);

            if (!_subs.CanUploadMedia(user.SubscriptionId))
                throw new Exception("Your subscription does not allow photo upload.");

            _photoAccessor.Add(photo);
        }

        public void Delete(int photoId, int userId)
        {
            // optional ownership enforcement
            _photoAccessor.Delete(photoId);
        }
    }

}
