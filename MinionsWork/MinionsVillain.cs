using System;
using System.Collections.Generic;

#nullable disable

namespace MinionsWork
{
    public partial class MinionsVillain
    {
        public int MinionId { get; set; }
        public int VillainId { get; set; }

        public virtual Minion Minion { get; set; }
        public virtual Villain Villain { get; set; }
    }
}
