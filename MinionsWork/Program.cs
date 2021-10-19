using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace MinionsWork
{
    class Program
    {
        static string connectionString = "Server=.;Database=Minions;Trusted_Connection=True";

        static void Main(string[] args) {
            //SelectVillains();
            //SelectMinionsByVillainId(int.Parse(Console.ReadLine() ?? throw new InvalidOperationException()));
            //WorkMenu();
            DeleteVillainById(int.Parse(Console.ReadLine() ?? string.Empty));
            //MinionsGrowUp(Console.ReadLine()?.Split(" ").Select(int.Parse).ToArray());
        }
        
        /// <summary>
        /// Миньоны взрослеют на один год.
        /// </summary>
        /// <param name="minions"></param>
        private static void MinionsGrowUp(int[] minions) {
            SqlConnection connection = new SqlConnection(connectionString);
            
            connection.Open();
            using (connection) {
                SqlCommand command;
                for (int i = 0; i < minions.Length; i++) {
                    string updateCommand = "UPDATE [Minions] SET [Age] = [Age] + 1 WHERE [Id] = @id";
                    command = new SqlCommand(updateCommand, connection);
                    command.Parameters.AddWithValue("@id", minions[i]);
                    command.ExecuteNonQuery();
                }

                string selectCommand = "SELECT * FROM [Minions]";
                command = new SqlCommand(selectCommand, connection);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    int j = 1;
                    while (reader.Read()) {
                        Console.WriteLine($"{j++}. {reader[1]} {reader[2]}");
                    }
                }
            }
        }

        /// <summary>
        /// Удаляет злодея из базы, освобождая миньонов от служения указанному злодею.
        /// </summary>
        /// <param name="id"></param>
        private static void DeleteVillainById(int id) {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using (connection) {
                string selectCommand = "SELECT * FROM [Villains] WHERE [Id] = @id";
                SqlCommand command = new SqlCommand(selectCommand, connection);
                command.Parameters.AddWithValue("@id", id);

                string name = "";
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    if (!reader.HasRows) {
                        Console.WriteLine($"Злодея с ID {id} в базе нет.");
                        return;
                    }
                    else {
                        reader.Read();
                        name = (string) reader[1];
                    }
                }
                
                int count = 0;
                selectCommand = "SELECT * FROM [MinionsVillains] WHERE [VillainId] = @id";
                command = new SqlCommand(selectCommand, connection);
                command.Parameters.AddWithValue("@id", id);
                SqlDataReader countReader = command.ExecuteReader();
                using (countReader) {
                    while (countReader.Read()) {
                        count++;
                    }
                }
                        
                string deleteDependenciesCommand = "DELETE FROM [MinionsVillains] WHERE [VillainId] = @id";
                command = new SqlCommand(deleteDependenciesCommand, connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();

                string deleteVillainCommand = "DELETE FROM [Villains] WHERE [Id] = @id";
                command = new SqlCommand(deleteVillainCommand, connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
                        
                Console.WriteLine($"{name} был удалён.");
                Console.WriteLine($"{count} миньонов было освобождено.");
            }
        }

        /// <summary>
        /// Меню для добавления миньёнов для служения указанному злодею.
        /// </summary>
        static void WorkMenu() {
            Console.Write("Minion (name, age, name of city): ");
            var minion = Console.ReadLine()?.Split(" ");
            Console.Write("\nVillain (name): ");
            string villain = Console.ReadLine();

            int villainId = CheckVillain(villain);
            int minionId = SetMinion(minion?[0], int.Parse(minion?[1] ?? string.Empty), minion?[2]);

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                string insertCommand =
                    "INSERT INTO [MinionsVillains] (MinionId, VillainId) VALUES(@minionId, @villainId)";
                SqlCommand command = new SqlCommand(insertCommand, connection);
                command.Parameters.AddWithValue("@minionId", minionId);
                command.Parameters.AddWithValue("@villainId", villainId);
                command.ExecuteNonQuery();
            }

            Console.WriteLine($"Миньён {minion?[0]} был успешно добавлен, чтобы служить {villain}");
        }

        /// <summary>
        /// Добавление миньона. Если указанного города нет в базе, то он добавляется автоматически
        /// Возвращает ID миньона.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <param name="town"></param>
        /// <returns></returns>
        private static int SetMinion(string name, int age, string town) {
            int townId = CheckTown(town);
            int minionId = 0;

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                string insertCommand = "INSERT INTO [Minions] (Name, Age, TownId) VALUES(@name, @age, @townId)";
                SqlCommand command = new SqlCommand(insertCommand, connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@age", age);
                command.Parameters.AddWithValue("@townId", townId);
                command.ExecuteNonQuery();

                string selectCommand = "SELECT [Id] FROM [Minions] WHERE [Name] = @name";
                command = new SqlCommand(selectCommand, connection);
                command.Parameters.AddWithValue("@name", name);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    reader.Read();
                    minionId = (int) reader[0];
                }
            }

            return minionId;
        }

        /// <summary>
        /// Проверка, есть ли такой город в базе данных. Если есть, то возвращает его Id, если нет - сохраняет в БД
        /// и возвращает Id только что зарегистрированного города.
        /// </summary>
        /// <param name="town"></param>
        /// <returns></returns>
        private static int CheckTown(string town) {
            int id = 0;

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                string selectCommand = "SELECT * FROM [Towns] WHERE [Name] = @name";
                SqlCommand command = new SqlCommand(selectCommand, connection);
                command.Parameters.AddWithValue("@name", town);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    if (reader.HasRows) {
                        reader.Read();
                        id = (int) reader[0];
                    }
                    else {
                        id = CreateTown(town);
                        Console.WriteLine($"Город {town} был успешно добавлен в базу.");
                    }
                }
            }

            return id;
        }

        /// <summary>
        /// Добавление города в базу. Страна не присваивается.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static int CreateTown(string name) {
            int id = 0;

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                string insertCommand = "INSERT INTO [Towns] (Name) VALUES(@name)";
                SqlCommand command = new SqlCommand(insertCommand, connection);
                command.Parameters.AddWithValue("@name", name);
                command.ExecuteNonQuery();

                string selectCommand = "SELECT [Id] FROM [Towns] WHERE [Name] = @name";
                command = new SqlCommand(selectCommand, connection);
                command.Parameters.AddWithValue("@name", name);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    reader.Read();
                    id = (int) reader[0];
                }
            }

            return id;
        }

        /// <summary>
        /// Метод проверяет, существует ли в БД злодей с заданным именем. Возвращает его ID.
        /// Если злодея с таким именем нет, то регистрирует его в базе и возвращает ID.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static int CheckVillain(string name) {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                int id = 0;
                string selectCommand = "SELECT * FROM [Villains] AS v WHERE [v].[Name] = @name";
                SqlCommand command = new SqlCommand(selectCommand, connection);
                command.Parameters.AddWithValue("@name", name);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    if (reader.HasRows) {
                        reader.Read();
                        id = (int) reader[0];
                    }
                    else {
                        id = SetVillain(name);
                        Console.WriteLine($"Злодей {name} был добавлен в базу данных.");
                    }
                }

                return id;
            }
        }

        /// <summary>
        /// Регистрирует в базе данных нового злодея со степенью злобы: "зло". Возвращает
        /// ID зарегистрированного злодея.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static int SetVillain(string name) {
            int id = 0;
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                string insertCommand = "INSERT INTO [Villains] (Name, EvilnessFactorId) VALUES(@name, @efactorId)";
                SqlCommand command = new SqlCommand(insertCommand, connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@efactorId", 4);
                command.ExecuteNonQuery();

                string selectCommand = "SELECT [Villains].[Id] FROM [Villains] WHERE [Villains].[Name] = @name";
                command = new SqlCommand(selectCommand, connection);
                command.Parameters.AddWithValue("@name", name);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    reader.Read();
                    id = (int) reader[0];
                }
            }

            return id;
        }

        /// <summary>
        /// Выводит на экран всех миньёнов, которые служат злодею с указанным Id.
        /// Если такого злодея нет, выведется соответствующее сообщение.
        /// </summary>
        /// <param name="id"></param>
        static void SelectMinionsByVillainId(int id) {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                string selectCommand = "SELECT * FROM [Villains] AS v WHERE [v].[Id] = @id";
                SqlCommand command = new SqlCommand(selectCommand, connection);
                command.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    if (!reader.HasRows) {
                        Console.WriteLine($"В базе данных нет злодея с ID {id}");
                        return;
                    }

                    reader.Read();
                    Console.WriteLine($"Villain: {reader[1]}");
                }

                selectCommand = "SELECT [Name], [Age] FROM [Minions] " +
                                "JOIN (SELECT [MinionId] FROM [MinionsVillains] " +
                                "WHERE [VillainId] = @id) AS e " +
                                "ON [e].[MinionId] = [Minions].[Id] ORDER BY [Minions].[Name]";
                command = new SqlCommand(selectCommand, connection);
                command.Parameters.AddWithValue("@id", id);
                reader = command.ExecuteReader();
                using (reader) {
                    if (reader.HasRows) {
                        int j = 1;
                        while (reader.Read()) {
                            Console.WriteLine($"{j++}. {reader[0]} {reader[1]}");
                        }
                    }
                    else {
                        Console.WriteLine("Миньонов нет.");
                    }
                }
            }

            connection.Close();
        }

        /// <summary>
        /// Самый простой запрос, выборка всех без фильтров
        /// </summary>
        static void Select() {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                string selectionCommandString = "SELECT * FROM Minions";
                SqlCommand command = new SqlCommand(selectionCommandString, connection);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    while (reader.Read()) {
                        for (int i = 0; i < reader.FieldCount; i++) {
                            Console.Write($"{reader[i]} ");
                        }

                        Console.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// Выбирает из базы всех злодеев, у которых в служении более 3-х миньонов.
        /// Использует представление Tab.
        /// </summary>
        static void SelectVillains() {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                // CREATE VIEW Tab AS SELECT MinionsVillains.VillainId, COUNT(MinionsVillains.MinionId) AS 'C' 
                // FROM [MinionsVillains] GROUP BY MinionsVillains.VillainId HAVING(COUNT(MinionsVillains.MinionId) > 3);
                string selectionCommandString = "SELECT Villains.Name, Tab.C FROM [Tab], [Villains] WHERE " +
                                                "Villains.Id = Tab.VillainId " +
                                                "ORDER BY Tab.C DESC";
                SqlCommand command = new SqlCommand(selectionCommandString, connection);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    while (reader.Read()) {
                        Console.WriteLine($"{reader.GetString(0)} - {reader.GetInt32(1)}");
                    }
                }
            }
        }

        /// <summary>
        /// Вставка нескольких записей. Так делать плохо, потенциальная SQL инъекция!
        /// </summary>
        static void BadInsert() {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                string creationCommandString = "INSERT INTO Minions VALUES" +
                                               "('Kent',22,1), " +
                                               "('Paul',34,2)";
                SqlCommand createCommand = new SqlCommand(creationCommandString, connection);
                Console.WriteLine(createCommand.ExecuteNonQuery());
            }
        }

        /// <summary>
        /// Вставка с параметрами, защищаемся от SQL инъекции!
        /// </summary>
        static void GoodInsert(string name, int age, int townId) {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection) {
                SqlCommand cmd = new SqlCommand("INSERT INTO Minions " +
                                                "(Name, Age, TownId) VALUES " +
                                                "(@name, @age, @townId)", connection);

                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@age", age);
                cmd.Parameters.AddWithValue("@townId", townId);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Выборка с параметрами, защищаемся от SQL инъекции!
        /// </summary>
        static void GoodSelect(string name) {
            string selectionCommandString = $"SELECT * FROM Minions " +
                                            $"WHERE Name = @name";
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(selectionCommandString, connection);
            SqlParameter parameter = new SqlParameter("@name", SqlDbType.NVarChar, 50) {Value = name};
            command.Parameters.Add(parameter);
            // Или параметр можно связать так
            // command.Parameters.AddWithValue("@name", name);

            connection.Open();
            using (connection) {
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    while (reader.Read()) {
                        for (int i = 0; i < reader.FieldCount; i++) {
                            Console.Write($"{reader[i]} ");
                        }

                        Console.WriteLine();
                    }
                }
            }
        }
    }
}