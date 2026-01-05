using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TravelJournal.Services.Exceptions
{
    [Serializable]
    public class SubscriptionLimitException : Exception
    {
        public SubscriptionLimitException() { }
        public SubscriptionLimitException(string message) : base(message) { }
        public SubscriptionLimitException(string message, Exception inner) : base(message, inner) { }
    }
}
