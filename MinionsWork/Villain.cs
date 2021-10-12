using System;
using System.Collections.Generic;

#nullable disable

namespace MinionsWork
{
    public partial class Villain
    {
        public Villain()
        {
            MinionsVillains = new HashSet<MinionsVillain>();
        }

        public Villain(string name, int evilnessFactorId) {
            Name = name;
            EvilnessFactorId = evilnessFactorId;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int EvilnessFactorId { get; set; }

        public virtual EvilnessFactor EvilnessFactor { get; set; }
        public virtual ICollection<MinionsVillain> MinionsVillains { get; set; }
    }
}
