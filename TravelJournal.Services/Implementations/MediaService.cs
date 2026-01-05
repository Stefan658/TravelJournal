using System;
using System.Collections.Generic;
using System.Linq;
using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using NLog;

namespace TravelJournal.Services.Implementations
{
    public class MediaService : IMediaService
    {
        private readonly IMediaAccessor _mediaAccessor;
        private readonly IUserAccessor _userAccessor;
        private readonly ISubscriptionService _subs;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public MediaService(IMediaAccessor mediaAccessor, IUserAccessor userAccessor, ISubscriptionService subs)
        {
            _mediaAccessor = mediaAccessor;
            _userAccessor = userAccessor;
            _subs = subs;
        }

        public IEnumerable<Media> GetByEntry(int entryId)
        {
            logger.Info($"[MediaService] Retrieving media for EntryId={entryId}");

            try
            {
                var media = _mediaAccessor.GetByEntry(entryId).ToList();
                logger.Info($"[MediaService] Retrieved {media.Count} media files for EntryId={entryId}");
                return media;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[MediaService] Error retrieving media for EntryId={entryId}");
                throw;
            }
        }

        public void Upload(Media media, int userId)
        {
            logger.Info($"[MediaService] Attempting to upload media for UserId={userId}, EntryId={media.EntryId}");

            try
            {
                var user = _userAccessor.GetById(userId);

                if (!_subs.CanUploadMedia(user.SubscriptionId))
                {
                    logger.Warn($"[MediaService] UserId={userId} is not allowed to upload media");
                    throw new Exception("Your subscription does not allow uploading images.");
                }

                _mediaAccessor.Add(media);
                logger.Info($"[MediaService] Media uploaded successfully (MediaId={media.MediaId})");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[MediaService] Error uploading media");
                throw;
            }
        }

        public void Delete(int id)
        {
            logger.Info($"[MediaService] Deleting MediaId={id}");

            try
            {
                _mediaAccessor.Delete(id);
                logger.Info($"[MediaService] MediaId={id} deleted successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[MediaService] Error deleting MediaId={id}");
                throw;
            }
        }
    }
}
