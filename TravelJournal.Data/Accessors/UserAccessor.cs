using System;
using System.Collections.Generic;
using System.Linq;
using TravelJournal.Domain.Entities;
using TravelJournal.Data.Context;
using NLog;

namespace TravelJournal.Data.Accessors
{
    public class UserAccessor : IUserAccessor
    {
        private readonly TravelJournalDbContext _db;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public UserAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<User> GetAll()
        {
            logger.Info("[UserAccessor] Retrieving all users");

            try
            {
                var users = _db.Users.ToList();
                logger.Info($"[UserAccessor] Retrieved {users.Count} users");
                return users;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[UserAccessor] Error retrieving users");
                throw;
            }
        }

        public User GetById(int id)
        {
            logger.Info($"[UserAccessor] GetById called for UserId={id}");

            try
            {
                var user = _db.Users.Find(id);

                if (user == null)
                    logger.Warn($"[UserAccessor] UserId={id} not found");

                return user;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[UserAccessor] Error retrieving UserId={id}");
                throw;
            }
        }

        public void Add(User user)
        {
            logger.Info($"[UserAccessor] Adding new user '{user.Username}'");

            try
            {
                _db.Users.Add(user);
                _db.SaveChanges();
                logger.Info($"[UserAccessor] User added successfully (UserId={user.UserId})");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[UserAccessor] Error adding user");
                throw;
            }
        }

        public void Update(User user)
        {
            logger.Info($"[UserAccessor] Updating UserId={user.UserId}");

            try
            {
                _db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                logger.Info($"[UserAccessor] UserId={user.UserId} updated successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[UserAccessor] Error updating UserId={user.UserId}");
                throw;
            }
        }

        public void Delete(int id)
        {
            logger.Info($"[UserAccessor] Deleting UserId={id}");

            try
            {
                var user = _db.Users.Find(id);

                if (user == null)
                {
                    logger.Warn($"[UserAccessor] Delete failed — UserId={id} not found");
                    return;
                }

                _db.Users.Remove(user);
                _db.SaveChanges();
                logger.Info($"[UserAccessor] UserId={id} deleted successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[UserAccessor] Error deleting UserId={id}");
                throw;
            }
        }
    }
}
