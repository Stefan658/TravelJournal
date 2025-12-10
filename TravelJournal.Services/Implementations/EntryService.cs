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
    public class EntryService : IEntryService
    {
        private readonly IEntryAccessor _entryAccessor;
        private readonly IUserAccessor _userAccessor;
        private readonly ISubscriptionService _subs;

        public EntryService(IEntryAccessor entryAccessor, IUserAccessor userAccessor, ISubscriptionService subs)
        {
            _entryAccessor = entryAccessor;
            _userAccessor = userAccessor;
            _subs = subs;
        }

        public IEnumerable<Entry> GetByJournal(int journalId)
            => _entryAccessor.GetAllByJournal(journalId);

        public Entry GetById(int id)
            => _entryAccessor.GetById(id);

        public void Create(Entry entry, int userId)
        {
            var user = _userAccessor.GetById(userId);
            var subscription = _subs.GetById(user.SubscriptionId);

            // verificăm limitele
            var existingEntriesCount = _entryAccessor
                .GetAllByJournal(entry.JournalId)
                .Count();

            if (existingEntriesCount >= subscription.EntryLimit)
                throw new Exception("Entry limit reached for your subscription plan.");

            _entryAccessor.Add(entry);
        }

        public void Update(Entry entry) => _entryAccessor.Update(entry);

        public void Delete(int id) => _entryAccessor.Delete(id);
    }
}
