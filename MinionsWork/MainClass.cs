using System;
using System.Linq;

namespace MinionsWork
{
    public class MainClass
    {
        static void Main(string[] args) {
            //SelectVillains();
            SelectMinionsByVillainId(int.Parse(Console.ReadLine()));
        }

        static void SelectVillains() {
            using (var context = new MinionsContext()) {
                //var res = context.Villains.Select(villain => villain.Name).ToList();
                var res = from Villains in context.Villains
                    join Tab in context.Tabs on Villains.Id equals Tab.VillainId
                    select new {
                        Villains.Name,
                        Tab.C
                    };
                foreach (var e in res) {
                    Console.WriteLine($"{e.Name} - {e.C}");
                }
            }
        }

        static void SelectMinionsByVillainId(int id) {
            using (var context = new MinionsContext()) {
                var res_1 = from Villain in context.Villains
                    where Villain.Id == id
                    select new {
                        Villain.Name
                    };
                if (res_1.ToArray().Length != 0) {
                    Console.WriteLine($"Villain: {res_1.ToArray()[0].Name}");
                    // SELECT [Name], [Age] FROM [Minions] 
                    // JOIN (SELECT [MinionId] FROM [MinionsVillains] WHERE [VillainId] = 1) AS [e]
                    // ON [e].[MinionId] = [Minions].[Id] ORDER BY [Minions].[Name]
                    var res = from minion in context.Minions
                        orderby minion.Name
                        join minionsvillain in context.MinionsVillains on minion.Id equals minionsvillain.MinionId
                        where minionsvillain.VillainId == id
                        select new {
                            minion.Name, minion.Age
                        };
                    if (res.ToArray().Length > 0) {
                        int j = 1;
                        foreach (var v in res) {
                            Console.WriteLine($"{j++}. {v.Name} {v.Age}");
                        }
                    }
                    else {
                        Console.WriteLine("no minions");
                    }
                }
                else {
                    Console.WriteLine($"В базе данных не существует злодея с идентификатором {id}");
                }
            }
        }
    }
}