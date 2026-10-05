using System;
using System.Collections;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace code
{
    public class Program
    {
        public static byte[] encryptedData;
        public static List<string> VaultList = new List<string>();
        public static byte[] MasterKey = null;
        //public static string test; // для теста

        public static void Main()
        {
            // начальное подготовка
            Console.WriteLine("Hello, this cruel world!");
            /* тест
            test = Console.ReadLine();
            WraitFile(encryptedData);
            WriteText(test, "test.txt");
            */
            LogicalProgramm();
        }


        public static void LogicalProgramm()
        {
            if (File.Exists("password.dat"))
            {
                Console.Clear();
                Console.WriteLine("=== ОБНАРУЖЕНА БАЗА ДАННЫХ ===");
                Console.WriteLine("Введите ваш прошлый Мастер-Ключ для разблокировки.");
                Console.Write("Вводите сударь: ");
                string masterKeyInput = Console.ReadLine();

                if (string.IsNullOrEmpty(masterKeyInput))
                {
                    Console.WriteLine("Ключ не может быть пустым! Перезапустите программу.");
                    Console.ReadKey();
                    return;
                }

                bool isKeyCorrect = false;

                try
                {
                    // Перенос конвертации внутрь try защищает от вылета при вводе не-Base64 строк вроде "вапа"
                    MasterKey = int.TryParse(masterKeyInput, out int keyInt)
                        ? BitConverter.GetBytes(keyInt)
                        : Convert.FromBase64String(masterKeyInput);

                    string base64FromFile = File.ReadAllText("password.dat");
                    byte[] encryptedBytes = Convert.FromBase64String(base64FromFile);
                    byte[] decryptedBytes = ShifrInOut(encryptedBytes, MasterKey);
                    string decryptedText = ByteToText(decryptedBytes);

                    // Разрезаем текст на строчки
                    string[] separatedPasswords = decryptedText.Split('\n');

                    // Проверяем контрольное слово на ПЕРВОЙ строчке массива [0]
                    if (separatedPasswords.Length > 0 && separatedPasswords[0] == "яблоко")
                    {
                        isKeyCorrect = true;

                        // Загружаем в память ВСЕ строчки, КРОМЕ первой проверочной
                        VaultList.Clear();
                        for (int i = 1; i < separatedPasswords.Length; i++)
                        {
                            VaultList.Add(separatedPasswords[i]);
                        }

                        Console.WriteLine("\n[База успешно разблокирована и загружена!]");
                        Console.WriteLine("Нажмите любую клавишу для перехода в главное меню...");
                        Console.ReadKey();
                    }
                }
                catch (Exception)
                {
                    // Перехватываем FormatException от некорректного Base64 и любые другие сбои
                    isKeyCorrect = false;
                }

                // Если контрольное слово не совпало или упал Exception — включаем твое меню спасения
                if (!isKeyCorrect)
                {
                    Console.WriteLine("\n[КРИТИЧЕСКАЯ ОШИБКА: Неверный ключ! Контрольное слово не совпало.]");
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("Что желаете сделать, сударь?");
                    Console.WriteLine("1 - Попробовать ввести ключ заново");
                    Console.WriteLine("2 - Полностью стереть базу данных и начать с нуля");
                    Console.WriteLine("Любая другая клавиша - Выход из программы");
                    Console.WriteLine("--------------------------------------------------");
                    Console.Write("Ваш выбор: ");

                    ConsoleKeyInfo failChoice = Console.ReadKey(true);

                    if (failChoice.Key == ConsoleKey.D1 || failChoice.Key == ConsoleKey.NumPad1)
                    {
                        LogicalProgramm(); // Перезапуск экрана ввода
                        return;
                    }
                    else if (failChoice.Key == ConsoleKey.D2 || failChoice.Key == ConsoleKey.NumPad2)
                    {
                        Console.WriteLine("\n\nВы уверены, что хотите БЕЗВОЗВРАТНО СТЕРЕТЬ все пароли? (Y/N)");
                        ConsoleKeyInfo confirm = Console.ReadKey(true);

                        if (confirm.Key == ConsoleKey.Y)
                        {
                            if (File.Exists("password.dat"))
                            {
                                File.Delete("password.dat");
                            }
                            VaultList.Clear();
                            MasterKey = null;
                            Console.WriteLine("\nБаза данных успешно уничтожена. Перезапускаем программу...");
                            Console.ReadKey();
                            LogicalProgramm();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("\nУничтожение отменено. Выход из программы...");
                            Console.ReadKey();
                            return;
                        }
                    }
                    else
                    {
                        return; // Выход из программы
                    }
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine("=== ДОБРО ПОЖАЛОВАТЬ ===");
                Console.WriteLine("У вас пока нет базы паролей. Она создастся автоматически при первом сохранении.");
                Console.WriteLine("Нажмите любую клавишу, чтобы начать...");
                Console.ReadKey();
            }

            StartScreen();
        }



        public static void StartScreen()
        {
            Console.Clear();
            Console.WriteLine("===========Менеджер паролей===============");
            Console.WriteLine("  Введите:");
            Console.WriteLine("  1 - для зашифровки");
            Console.WriteLine("  2 - для расшифровки");
            Console.WriteLine("  3 - для изменения Мастер-Ключа"); // Новый пункт!
            Console.WriteLine("  4 - для выхода (совсем)");
            Console.WriteLine("==========================================");

            Console.Write("Вводите сударь: ");
            string input = Console.ReadLine();

            // Поменяли проверку: теперь диапазон от 1 до 4!
            if (int.TryParse(input, out int valueUs) && valueUs < 5 && valueUs > 0)
            {
                Console.WriteLine($"Выбор только за вами))): {valueUs}");
            }
            else
            {
                Console.WriteLine("Ошибка! Вы ввели не число, а какую-то ерунду.");
                Console.ReadKey();
                StartScreen();
                return;
            }

            if (valueUs == 4) Environment.Exit(0); // Выход теперь на 4
            if (valueUs == 1) ShiftScreen();
            if (valueUs == 2) UnShifrScreen();
            if (valueUs == 3) ChangeKeyScreen(); // Вызов нового экрана смены ключа
        }

        public static void ShiftScreen()
        {
            Console.Clear();
            Console.WriteLine("=== ДОБАВЛЕНИЕ НОВОГО ПАРОЛЯ ===");
            Console.WriteLine("Введите новый пароль или текст:");
            string usText = Console.ReadLine();

            if (string.IsNullOrEmpty(usText))
            {
                Console.WriteLine("Как-то пусто((\nДавай заново");
                Console.ReadKey();
                ShiftScreen();
                return;
            }

            // Добавляем запись в память
            VaultList.Add(usText);

            // [МАГИЯ ТУТ]: Склеиваем базу, добавляя "яблоко" на самую первую строчку!
            string allPasswordsText = "яблоко\n" + string.Join("\n", VaultList);

            byte[] usBayt = TextToByte(allPasswordsText);

            if (MasterKey == null)
            {
                Console.Write("Вы шифруете базу ПЕРВЫЙ РАЗ. Введите числовой ключ (или Enter для рандома): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int userKeyInt))
                {
                    MasterKey = BitConverter.GetBytes(userKeyInt);
                    Console.WriteLine($"Ваш установленный ключ: {userKeyInt}. ЗАПОМНИТЕ ЕГО!");
                }
                else
                {
                    MasterKey = GenerateRandomKey(usBayt.Length);
                    string keyBase64 = Convert.ToBase64String(MasterKey);
                    Console.WriteLine($"Ваш случайный Мастер-Ключ ЗАПОМНИТЕ ЕГО!!!!!\n{keyBase64}");
                }
            }

            // Шифруем единым Мастер-Ключом
            byte[] encryptedBytes = ShifrInOut(usBayt, MasterKey);
            string base64Text = Convert.ToBase64String(encryptedBytes);

            WriteText(base64Text, "password.dat");

            Console.WriteLine($"\nУспешно сохранено! Всего записей в базе: {VaultList.Count}");
            Console.WriteLine("Нажмите клавишу для возврата в меню");
            Console.ReadKey();
            StartScreen();
        }

        public static void ChangeKeyScreen()
        {
            Console.Clear();
            Console.WriteLine("=== СМЕНА МАСТЕР-КЛЮЧА ===");

            // Если базы еще нет, менять ключ бессмысленно
            if (!File.Exists("password.dat") || MasterKey == null)
            {
                Console.WriteLine("У вас еще нет созданной базы данных! Сначала добавьте первый пароль.");
                Console.WriteLine("Нажмите клавишу для возврата в меню...");
                Console.ReadKey();
                StartScreen();
                return;
            }

            Console.WriteLine("Введите НОВЫЙ числовой ключ (или Enter для генерации рандомного):");
            Console.Write("Новый ключ: ");
            string input = Console.ReadLine();

            byte[] newKey;

            // Формируем новый ключ
            if (int.TryParse(input, out int newKeyInt))
            {
                newKey = BitConverter.GetBytes(newKeyInt);
                Console.WriteLine($"Ваш НОВЫЙ ключ: {newKeyInt}. ЗАПОМНИТЕ ЕГО!");
            }
            else
            {
                // Склеиваем текущую базу, чтобы узнать нужную длину ключа
                string allText = "яблоко\n" + string.Join("\n", VaultList);
                byte[] tempBytes = TextToByte(allText);

                newKey = GenerateRandomKey(tempBytes.Length);
                string keyBase64 = Convert.ToBase64String(newKey);
                Console.WriteLine($"Ваш НОВЫЙ случайный Мастер-Ключ ЗАПОМНИТЕ ЕГО!!!!!\n{keyBase64}");
            }

            Console.WriteLine("\nПерешифровываем базу данных с новым ключом...");

            try
            {
                // Склеиваем данные из памяти обратно в один текст с контрольным словом
                string allPasswordsText = "яблоко\n" + string.Join("\n", VaultList);
                byte[] usBayt = TextToByte(allPasswordsText);

                // Шифруем НОВЫМ ключом
                byte[] encryptedBytes = ShifrInOut(usBayt, newKey);
                string base64Text = Convert.ToBase64String(encryptedBytes);

                // Перезаписываем файл
                WriteText(base64Text, "password.dat");

                // ВАЖНО: Обновляем ключ в оперативной памяти программы!
                MasterKey = newKey;

                Console.WriteLine("Мастер-Ключ успешно изменен!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при перешифровании: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
            StartScreen();
        }



        public static void UnShifrScreen()
        {
            Console.Clear();
            Console.WriteLine("=== ВАШИ СОХРАНЕННЫЕ ПАРОЛИ ===");
            Console.WriteLine("---------------------------------");

            if (VaultList.Count == 0)
            {
                Console.WriteLine("База данных пока пуста.");
            }
            else
            {
                int counter = 1;
                foreach (string password in VaultList)
                {
                    Console.WriteLine($"{counter}. {password}");
                    counter++;
                }
            }

            Console.WriteLine("---------------------------------");
            Console.WriteLine("Нажмите любую клавишу для возврата в меню");
            Console.ReadKey();
            StartScreen();
        }





        public static byte[] GenerateRandomKey(int length = 20)
        {
            byte[] key = new byte[length];

            // Заполняет массив абсолютно случайными криптографическими байтами
            RandomNumberGenerator.Fill(key);

            return key;
        }
        public static void WraitFile(byte[] data, string name = "password.dat")
        {
            if (encryptedData != null && !string.IsNullOrEmpty(name))
            {
                if (!Path.HasExtension(name)) name += ".dat";
                using (FileStream fs = new FileStream(name, FileMode.Append, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                }
                Console.WriteLine("Файл успешно создан!");
            }
            else Console.WriteLine("пока пусто)");
        }

        public static void WriteText(string text, string name = "password.dat")
        {
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(name))
            {
                if (!Path.HasExtension(name)) name += ".dat"; // защита от дурака

                File.WriteAllText(name, text);
                Console.WriteLine("Записано");
            }
            else Console.WriteLine("Ошибка чтото пусто");
        }

        public static string ReadTextStr(string name = "password.dat")
        {
            string loadedText = null;
            if (!string.IsNullOrEmpty(name) && File.Exists(name))
            {
                loadedText = File.ReadAllText(name);
                return loadedText;
            }
            else
            {
                 return null; Console.WriteLine($"Ошибка: файл '{name}' не найден или имя пустое.");
            }

        }

        public static byte[] TextToByte(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                return bytes;
            }
            Console.WriteLine("Ошибка преобразования текста в байты! (не критично)");
            return null;
        }
        public static string ByteToText(byte[] bytes)
        {
            if (bytes != null)
            {
                string text = Encoding.UTF8.GetString(bytes);
                return text;
            }
            Console.WriteLine("Ошибка преобразования байтов в текст! (не критично)");
            return null;
            
        }

        public static byte[] ShifrInOut(byte[] data, byte[] key)
        {
            if (data == null || key == null || key.Length == 0) { return null; Console.WriteLine("Ошибка шифрования (не критично для работы)"); }
            byte[] rezult = new byte[data.Length];
            for(int i = 0; i < data.Length; i++)
            {
                rezult[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return rezult;
        }
        
    }
}
