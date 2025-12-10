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
    public class UserService : IUserService
    {
        private readonly IUserAccessor _userAccessor;
        private readonly ISubscriptionAccessor _subscriptionAccessor;

        public UserService(IUserAccessor userAccessor, ISubscriptionAccessor subscriptionAccessor)
        {
            _userAccessor = userAccessor;
            _subscriptionAccessor = subscriptionAccessor;
        }

        public IEnumerable<User> GetAll()
        {
            return _userAccessor.GetAll();
        }

        public User GetById(int id)
        {
            return _userAccessor.GetById(id);
        }

        public void Create(User user)
        {
            _userAccessor.Add(user);
        }

        public void Update(User user)
        {
            _userAccessor.Update(user);
        }

        public void Delete(int id)
        {
            _userAccessor.Delete(id);
        }

        // 🔥 Funcția principală pentru abonamente
        public Subscription GetSubscription(int userId)
        {
            var user = _userAccessor.GetById(userId);
            return _subscriptionAccessor.GetById(user.SubscriptionId);
        }
    }
}
