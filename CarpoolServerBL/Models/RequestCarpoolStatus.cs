using System;
using System.Collections.Generic;

#nullable disable

namespace CarpoolServerBL.Models
{
    public partial class RequestCarpoolStatus
    {
        public RequestCarpoolStatus()
        {
            RequestToJoinCarpools = new HashSet<RequestToJoinCarpool>();
        }

        public int RequestId { get; set; }
        public string RequestName { get; set; }

        public virtual ICollection<RequestToJoinCarpool> RequestToJoinCarpools { get; set; }
    }

    public enum CARPOOL_REQUEST_STATUS
    {
        APPROVED = 1,
        DECLINED = 2,
        NEW = 3
    }
}
