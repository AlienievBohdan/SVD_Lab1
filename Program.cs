using System;
using System.Data.SQLite;

namespace SVD_Lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source=lab.sqlite;Version=3;";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();

                using (SQLiteCommand pragmaCmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", m_dbConnection))
                {
                    pragmaCmd.ExecuteNonQuery();
                }

                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("ГОЛОВНЕ МЕНЮ");
                    Console.WriteLine("1. Додавання даних та перевірка виключення");
                    Console.WriteLine("2. Зміна даних");
                    Console.WriteLine("3. Видалення даних");
                    Console.WriteLine("4. Перевірка тригерів");
                    Console.WriteLine("0. Вихід");
                    Console.Write("Оберіть дію: ");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            try
                            {
                                new SQLiteCommand("DELETE FROM Students; DELETE FROM Groups;", m_dbConnection).ExecuteNonQuery();

                                string insertGroups = "INSERT INTO Groups (GroupName, Faculty, Headman) VALUES ('СН-31', 'ФІС', 'Іванов І.'), ('СН-32', 'ФІС', 'Петров П.');";
                                new SQLiteCommand(insertGroups, m_dbConnection).ExecuteNonQuery();

                                string insertStudents = @"
                                    INSERT INTO Students (CardNumber, FullName, BirthDate, AvgGrade, GroupName) VALUES 
                                    (1, 'Олексієнко О.', '2005-01-01', 85.5, 'СН-31'),
                                    (2, 'Михайленко М.', '2005-02-02', 90.0, 'СН-31'),
                                    (3, 'Сидоренко С.', '2004-10-10', 75.0, 'СН-32'),
                                    (4, 'Григоренко Г.', '2005-05-05', 88.0, 'СН-32');";
                                new SQLiteCommand(insertStudents, m_dbConnection).ExecuteNonQuery();
                                Console.WriteLine("Дані успішно додано (попередні тестові записи очищено)!");

                                Console.WriteLine("\nСпроба додати студента у неіснуючу групу 'СН-99'...");
                                string errorSql = "INSERT INTO Students (CardNumber, FullName, BirthDate, AvgGrade, GroupName) VALUES (99, 'Помилковий П.', '2000-01-01', 60.0, 'СН-99');";
                                new SQLiteCommand(errorSql, m_dbConnection).ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("УСПІШНО ПЕРЕХОПЛЕНО ПОМИЛКУ ЗОВНІШНЬОГО КЛЮЧА:\n" + ex.Message);
                            }
                            break;

                        case "2":
                            string updateSql = "UPDATE Groups SET GroupName = 'СН-31-Оновлена' WHERE GroupName = 'СН-31';";
                            int updateRows = new SQLiteCommand(updateSql, m_dbConnection).ExecuteNonQuery();

                            if (updateRows > 0)
                                Console.WriteLine("Назву групи 'СН-31' змінено на 'СН-31-Оновлена'. Тригер UPDATE CASCADE спрацював.");
                            else
                                Console.WriteLine("Помилка: групу 'СН-31' не знайдено в базі!");
                            break;

                        case "3":
                            string deleteSql = "DELETE FROM Groups WHERE GroupName = 'СН-32';";
                            int deleteRows = new SQLiteCommand(deleteSql, m_dbConnection).ExecuteNonQuery();

                            if (deleteRows > 0)
                                Console.WriteLine("Групу 'СН-32' видалено з головної таблиці. Тригер DELETE SET NULL спрацював.");
                            else
                                Console.WriteLine("Помилка: групу 'СН-32' не знайдено в базі!");
                            break;

                        case "4":
                            string selectSql = "SELECT CardNumber, FullName, GroupName FROM Students;";
                            using (SQLiteDataReader reader = new SQLiteCommand(selectSql, m_dbConnection).ExecuteReader())
                            {
                                Console.WriteLine("\nСписок студентів у базі:");
                                while (reader.Read())
                                {
                                    string group = reader["GroupName"] != DBNull.Value ? reader["GroupName"].ToString() : "NULL (Без групи)";
                                    Console.WriteLine($"Квиток: {reader["CardNumber"]}, Студент: {reader["FullName"]}, Група: {group}");
                                }
                            }
                            break;

                        case "0":
                            exit = true;
                            break;

                        default:
                            Console.WriteLine("Невірний вибір.");
                            break;
                    }
                }
            }
        }
    }
}