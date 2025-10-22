using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Enums
{
    public enum ShipmentStatus 
    { 
        Created = 0,
        PendingRecipientApproval = 1, 
        UnderReview = 2, 
        RepAssigned = 3, 
        RepOnWayToCollect = 4, 
        Collected = 5, 
        RepOnWayToDeliver = 6, 
        Delivered = 7, 
        Cancelled = 8 
    }
}
