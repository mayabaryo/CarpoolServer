using System;
using System.Collections.Generic;

#nullable disable

namespace CarpoolServerBL.Models
{
    public partial class Kid
    {
        public int Id { get; set; }

        public virtual User IdNavigation { get; set; }
    }
}
