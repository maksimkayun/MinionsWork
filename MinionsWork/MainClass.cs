using System;
using System.Linq;

namespace MinionsWork
{
    public class MainClass
    {
        static void Main(string[] args) {
            //SelectVillains();
            //SelectMinionsByVillainId(int.Parse(Console.ReadLine()));
            SetMinion("Eric", 9, "Baltimor");
        }
        
        /// <summary>
        /// Добавление миньёна. Если указанного города нет в базе, то он добавляется
        /// </summary>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <param name="city"></param>
        static void SetMinion(string name, int age, string city) {
            int townId = CheckCity(city);
            using (var context = new MinionsContext()) {
                var minion = new Minion(name, age, 1);
                context.Minions.Add(minion);
                context.SaveChanges();
            }

            using (var context = new MinionsContext()) {
                var res = from m in context.Minions select m;
                foreach (var v in res) {
                    Console.WriteLine($"{v.Name} {v.Age} {v.Town}");
                }
            }
        }
        
        /// <summary>
        /// Проверка, есть ли такой город в базе данных. Если есть, то возвращает его Id, если нет - сохраняет в БД
        /// и возвращает Id только что зарегистрированного города
        /// </summary>
        /// <param name="name"></param>
        static int CheckCity(string name) {
            using (var ctx = new MinionsContext()) {
                var res = from t in ctx.Towns
                    where t.Name == name
                    select t;
                if (res.ToArray().Length == 0) {
                    CreateTown(name);
                }
                else {
                    return res.ToArray()[0].Id;
                }
                res = from t in ctx.Towns
                    where t.Name == name
                    select t;
                return res.ToArray()[0].Id;
            }
        }
        
        /// <summary>
        /// Добавление города в базу с присваиванием рандомной страны
        /// </summary>
        /// <param name="name"></param>
        static void CreateTown(string name) {
            using (var context = new MinionsContext()) {
                var res = from с in context.Countries select с;
                int quantity = 0;
                foreach (var v in res) {
                    quantity++;
                }

                var town = new Town(name, new Random().Next(1, quantity));
                context.Towns.Add(town);
                context.SaveChanges();
            }
            Console.WriteLine($"Город {name} успешно добавлен!");
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