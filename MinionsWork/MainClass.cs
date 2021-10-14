using System;
using System.Linq;

namespace MinionsWork
{
    public class MainClass
    {
        static void Main(string[] args) {
            //SelectVillains();
            //SelectMinionsByVillainId(int.Parse(Console.ReadLine() ?? string.Empty));
            WorkMenu();
        }

        static void WorkMenu() {
            Console.Write("Minion (name, age, name of city): ");
            var minion = Console.ReadLine()?.Split(" ");
            Console.Write("\nVillain (name): ");
            string villain = Console.ReadLine();

            int villainId = CheckVillain(villain);
            int minionId = SetMinion(minion?[0], int.Parse(minion?[1] ?? string.Empty), minion?[2]);

            using (var context = new MinionsContext()) {
                var mv = new MinionsVillain(minionId, villainId);
                context.MinionsVillains.Add(mv);
                context.SaveChanges();
            }
            
            Console.WriteLine($"Миньён {minion?[0]} был успешно добавлен, чтобы служить {villain}");
        }
        
        /// <summary>
        /// Добавление миньёна. Если указанного города нет в базе, то он добавляется автоматически
        /// Возвращает ID миньёна
        /// </summary>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <param name="city"></param>
        static int SetMinion(string name, int age, string city) {
            int townId = CheckCity(city);
            int minionId = 0;
            using (var context = new MinionsContext()) {
                var minion = new Minion(name, age, townId);
                context.Minions.Add(minion);
                context.SaveChanges();
                var res = from m in context.Minions where m.Name == name select m;
                minionId = res.ToArray()[0].Id;
            }
            return minionId;
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
                var town = new Town(name);
                context.Towns.Add(town);
                context.SaveChanges();
            }
            Console.WriteLine($"Город {name} успешно добавлен!");
        }
        
        /// <summary>
        /// Метод проверяет, существует ли в БД злодей с заданным именем. Возвращает его ID.
        /// Если злодея с таким именем нет, то регистрирует его в базе и возвращает ID.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static int CheckVillain(string name) {
            int id = 0;
            using (var context = new MinionsContext()) {
                var res = from v in context.Villains where v.Name == name select v;
                if (res.ToArray().Length == 0) {
                    id = SetVillain(name);
                    Console.WriteLine($"Злодей {name} был добавлен в базу данных.");
                }
                else {
                    id = res.ToArray()[0].Id;
                }
            }
            return id;
        }
        
        /// <summary>
        /// Регистрирует в базе данных нового злодея со степенью злобы: "зло". Возвращает
        /// ID зарегистрированного злодея.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static int SetVillain(string name) {
            int id = 0;
            using (var context = new MinionsContext()) {
                var newVillain = new Villain(name, 4);
                context.Villains.Add(newVillain);
                context.SaveChanges();
                var res = from v in context.Villains where v.Name == name select v;
                id = res.ToArray()[0].Id;
            }
            return id;
        }

        /// <summary>
        /// Метод возвращает всех злодеев, которым служат более 3-х миньёнов.
        /// Для изменения критерия запроса необходимо изменить представление Tab.
        /// </summary>
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
        
        /// <summary>
        /// Выводит на экран всех миньёнов, которые служат злодею с указанным Id.
        /// Если такого злодея нет, выведется соответствующее сообщение.
        /// </summary>
        /// <param name="id"></param>
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