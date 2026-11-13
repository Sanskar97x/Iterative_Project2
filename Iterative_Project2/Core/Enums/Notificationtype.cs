using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Enums
{
    public enum Notificationtype
    {
        General = 0,
        ApplicationReceived = 1,        // employer: someone applied to a job
        ApplicationStatusChanged = 2,   // candidate: status of their application changed
        CompanyVerification = 3         // employer: company verified / unverified by an admin
    }
}
