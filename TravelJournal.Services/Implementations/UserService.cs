using System;
using System.Collections.Generic;
using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using NLog;
using System.Linq;

namespace TravelJournal.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserAccessor _userAccessor;
        private readonly ISubscriptionAccessor _subscriptionAccessor;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public UserService(IUserAccessor userAccessor, ISubscriptionAccessor subscriptionAccessor)
        {
            _userAccessor = userAccessor;
            _subscriptionAccessor = subscriptionAccessor;
        }

        public IEnumerable<User> GetAll()
        {
            logger.Info("[UserService] Retrieving all users");

            try
            {
                return _userAccessor.GetAll();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[UserService] Error retrieving users");
                throw;
            }
        }

        public User GetById(int id)
        {
            logger.Info($"[UserService] GetById called for UserId={id}");

            try
            {
                var user = _userAccessor.GetById(id);

                if (user == null)
                    logger.Warn($"[UserService] UserId={id} not found");

                return user;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[UserService] Error in GetById for UserId={id}");
                throw;
            }
        }

        public void Create(User user)
        {
            logger.Info($"[UserService] Creating user '{user.Username}'");

            try
            {
                _userAccessor.Add(user);
                logger.Info($"[UserService] User created successfully (UserId={user.UserId})");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[UserService] Error creating user");
                throw;
            }
        }

        public void Update(User user)
        {
            logger.Info($"[UserService] Updating UserId={user.UserId}");

            try
            {
                _userAccessor.Update(user);
                logger.Info($"[UserService] UserId={user.UserId} updated successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[UserService] Error updating UserId={user.UserId}");
                throw;
            }
        }

        public void Delete(int id)
        {
            logger.Info($"[UserService] Deleting UserId={id}");

            try
            {
                _userAccessor.Delete(id);
                logger.Info($"[UserService] UserId={id} deleted successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[UserService] Error deleting UserId={id}");
                throw;
            }
        }

        public Subscription GetSubscription(int userId)
        {
            logger.Info($"[UserService] Retrieving subscription for UserId={userId}");

            try
            {
                var user = _userAccessor.GetById(userId);

                if (user == null)
                {
                    logger.Warn($"[UserService] UserId={userId} not found for subscription lookup");
                    return null;
                }

                var sub = _subscriptionAccessor.GetById(user.SubscriptionId);
                logger.Info($"[UserService] Subscription '{sub?.Name}' retrieved for UserId={userId}");

                return sub;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[UserService] Error retrieving subscription");
                throw;
            }
        }

        public User GetByUsername(string username)
        {
            logger.Info($"[UserService] GetByUsername called for Username='{username}'");

            try
            {
                var users = _userAccessor.GetAll();
                var user = users.FirstOrDefault(u => u.Username == username);

                if (user == null)
                    logger.Warn($"[UserService] Username='{username}' not found");

                return user;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[UserService] Error in GetByUsername");
                throw;
            }
        }

    }
}
