using System;
using System.Collections.Generic;

#nullable disable

namespace CarpoolServerBL.Models
{
    public partial class RequestCarpoolStatus
    {
        public RequestCarpoolStatus()
        {
            KidsInCarpools = new HashSet<KidsInCarpool>();
        }

        public int RequestId { get; set; }
        public string RequestName { get; set; }

        public virtual ICollection<KidsInCarpool> KidsInCarpools { get; set; }
    }
}
