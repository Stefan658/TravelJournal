using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;

namespace TravelJournal.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Subscription GetById(int id);
        IEnumerable<Subscription> GetAll();
        bool CanUploadMedia(int subscriptionId);
        bool CanExportPdf(int subscriptionId);
        bool CanUseMap(int subscriptionId);
    }
}
