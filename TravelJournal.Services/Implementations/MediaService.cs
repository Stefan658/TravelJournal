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
    public class MediaService : IMediaService
    {
        private readonly IMediaAccessor _mediaAccessor;
        private readonly IUserAccessor _userAccessor;
        private readonly ISubscriptionService _subs;

        public MediaService(IMediaAccessor mediaAccessor, IUserAccessor userAccessor, ISubscriptionService subs)
        {
            _mediaAccessor = mediaAccessor;
            _userAccessor = userAccessor;
            _subs = subs;
        }

        public IEnumerable<Media> GetByEntry(int entryId)
            => _mediaAccessor.GetByEntry(entryId);

        public void Upload(Media media, int userId)
        {
            var user = _userAccessor.GetById(userId);

            if (!_subs.CanUploadMedia(user.SubscriptionId))
                throw new Exception("Your subscription does not allow uploading images.");

            _mediaAccessor.Add(media);
        }

        public void Delete(int id) => _mediaAccessor.Delete(id);
    }
}
