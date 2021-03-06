using System;
using System.Collections.Generic;

#nullable disable

namespace MinionsWork
{
    public partial class Minion
    {
        public Minion()
        {
            MinionsVillains = new HashSet<MinionsVillain>();
        }

        public Minion(string name, int age, int id) {
            Name = name;
            Age = age;
            TownId = id;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int TownId { get; set; }

        public virtual Town Town { get; set; }
        public virtual ICollection<MinionsVillain> MinionsVillains { get; set; }
    }
}
