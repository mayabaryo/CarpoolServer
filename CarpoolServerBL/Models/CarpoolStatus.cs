using System;
using System.Collections.Generic;

#nullable disable

namespace CarpoolServerBL.Models
{
    public partial class CarpoolStatus
    {
        public CarpoolStatus()
        {
            Carpools = new HashSet<Carpool>();
        }

        public int StatusId { get; set; }
        public string StatusName { get; set; }

        public virtual ICollection<Carpool> Carpools { get; set; }
    }
}
