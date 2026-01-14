using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TravelJournal.Domain.Entities;

namespace TravelJournal.Services.Interfaces
{
    public interface IUserService
    {
        User GetById(int id);
        IEnumerable<User> GetAll();
        void Create(User user);
        void Update(User user);
        void Delete(int id);

        Subscription GetSubscription(int userId);

        User GetByUsername(string username);

    }
}

