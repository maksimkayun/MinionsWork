using System;
using System.Linq;

namespace MinionsWork
{
    public class MainClass
    {
        static void Main(string[] args) {
            using (var context = new MinionsContext()) {
                //var res = context.Villains.Select(villain => villain.Name).ToList();
                var res = from Villains in context.Villains
                    join Tab in context.Tabs on Villains.Id equals Tab.VillainId
                    select new {
                        Villains.Name,
                        Tab.C
                    };
                foreach (var VARIABLE in res) {
                    Console.WriteLine(VARIABLE);
                }
            }
        }
    }
}