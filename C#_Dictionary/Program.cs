using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading;

internal class Dictionaries
{
    private string? languageTo;
    private string? languageFrom;
    private Dictionary<string, List<string>> dictionary;

    public Dictionaries(string? languageTo, string? languageFrom)
    {
        this.languageTo = languageTo;
        this.languageFrom = languageFrom;
        this.dictionary = new Dictionary<string, List<string>>();
    }

    public void AddWord(string word, List<string> translation)
    {
        if (dictionary.ContainsKey(word))
        {
            foreach (var item in translation)
            {
                if (!dictionary[word].Contains(item))
                {
                    dictionary[word].Add(item);
                }
            }
        }
        else
        {
            dictionary[word] = translation;
        }
    }

    public void RemoveWord(string word, string filename)
    {
        if (File.Exists(filename))
        {
            var lines = File.ReadAllLines(filename).ToList();
            bool removed = false;
            for (int i = 0; i < lines.Count; i++)
            {
                var parts = lines[i].Split(':');
                if (parts.Length == 2 && parts[0].Trim() == word)
                {
                    lines.RemoveAt(i);
                    removed = true;
                    break;
                }
            }
            if (removed)
            {
                File.WriteAllLines(filename, lines);
                Console.WriteLine($"Слово '{word}' удалено из словаря в файле '{filename}'.");
            }
            else
            {
                Console.WriteLine($"Слово '{word}' не найдено в словаре в файле '{filename}'.");
            }
        }
        else
        {
            Console.WriteLine($"Файл словаря '{filename}' не найден.");
        }
    }

    public void RemoveTranslation(string word, string translation, string filename)
    {
        if (File.Exists(filename))
        {
            var lines = File.ReadAllLines(filename).ToList();
            bool removed = false;
            for (int i = 0; i < lines.Count; i++)
            {
                var parts = lines[i].Split(':');
                if (parts.Length == 2 && parts[0].Trim() == word)
                {
                    var translations = parts[1].Split(',').Select(t => t.Trim()).ToList();
                    if (translations.Contains(translation))
                    {
                        if (translations.Count > 1)
                        {
                            translations.Remove(translation);
                            lines[i] = $"{word}: {string.Join(", ", translations)}";
                            removed = true;
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Невозможно удалить перевод '{translation}' для слова '{word}', так как это единственный перевод.");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Перевод '{translation}' для слова '{word}' не найден в словаре в файле '{filename}'.");
                        return;
                    }
                }
            }
            if (removed)
            {
                File.WriteAllLines(filename, lines);
                Console.WriteLine($"Перевод '{translation}' для слова '{word}' удален из словаря в файле '{filename}'.");
            }
            else
            {
                Console.WriteLine($"Слово '{word}' не найдено в словаре в файле '{filename}'.");
            }
        }
        else
        {
            Console.WriteLine($"Файл словаря '{filename}' не найден.");
        }
    }

    public void SearchWord(string word, string filename)
    {
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        string currentWord = parts[0].Trim();
                        if (currentWord == word)
                        {
                            Console.WriteLine($"{word} : {parts[1].Trim()}");
                            return;
                        }
                    }
                }
                Console.WriteLine($"Слово '{word}' не найдено в словаре '{filename}'.");
            }
        }
        else
        {
            Console.WriteLine($"Файл словаря '{filename}' не найден.");
        }
    }

    public void ReplaceWord(string oldWord, string newWord, string filename)
    {
        if (File.Exists(filename))
        {
            var lines = File.ReadAllLines(filename).ToList();
            bool replaced = false;
            for (int i = 0; i < lines.Count; i++)
            {
                var parts = lines[i].Split(':');
                if (parts.Length == 2 && parts[0].Trim() == oldWord)
                {
                    lines[i] = $"{newWord}: {parts[1].Trim()}";
                    replaced = true;
                    break;
                }
            }
            if (replaced)
            {
                File.WriteAllLines(filename, lines);
                Console.WriteLine($"Слово '{oldWord}' заменено на '{newWord}' в словаре в файле '{filename}'.");
            }
            else
            {
                Console.WriteLine($"Слово '{oldWord}' не найдено в словаре в файле '{filename}'.");
            }
        }
        else
        {
            Console.WriteLine($"Файл словаря '{filename}' не найден.");
        }
    }

    public void ReplaceTranslation(string word, string oldTranslation, string newTranslation, string filename)
    {
        if (File.Exists(filename))
        {
            var lines = File.ReadAllLines(filename).ToList();
            bool replaced = false;
            for (int i = 0; i < lines.Count; i++)
            {
                var parts = lines[i].Split(':');
                if (parts.Length == 2 && parts[0].Trim() == word)
                {
                    var translations = parts[1].Split(',').Select(t => t.Trim()).ToList();
                    if (translations.Contains(oldTranslation))
                    {
                        var index = translations.IndexOf(oldTranslation);
                        translations[index] = newTranslation;
                        lines[i] = $"{word}: {string.Join(", ", translations)}";
                        replaced = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Перевод '{oldTranslation}' для слова '{word}' не найден в словаре в файле '{filename}'.");
                        return;
                    }
                }
            }
            if (replaced)
            {
                File.WriteAllLines(filename, lines);
                Console.WriteLine($"Перевод '{oldTranslation}' для слова '{word}' заменен на '{newTranslation}' в словаре в файле '{filename}'.");
            }
            else
            {
                Console.WriteLine($"Слово '{word}' не найдено в словаре в файле '{filename}'.");
            }
        }
        else
        {
            Console.WriteLine($"Файл словаря '{filename}' не найден.");
        }
    }

    public void LoadDictionaryFromFile(string filename)
    {
        if (File.Exists(filename))
        {
            dictionary.Clear();
            using (StreamReader reader = new StreamReader(filename))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(':');
                    string word = parts[0].Trim();
                    List<string> translations = parts[1].Split(',').Select(t => t.Trim()).ToList();
                    dictionary[word] = translations;
                }
            }
        }
        else
        {
            Console.WriteLine("Файл со словарем не найден.");
        }
    }

    public void SaveDictionaryToFile(string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            foreach (var item in dictionary)
            {
                writer.WriteLine($"{item.Key}: {string.Join(", ", item.Value)}");
            }
        }
    }


    public void ExportDictionaryToFile(string selectedFile, string exportFileName)
    {
        if (File.Exists(selectedFile))
        {
            File.Copy(selectedFile, exportFileName, true);
            Console.WriteLine($"Словарь успешно экспортирован в файл '{exportFileName}'.");
        }
        else
        {
            Console.WriteLine($"Файл словаря '{selectedFile}' не найден.");
        }
    }

    public Dictionary<string, List<string>> GetDictionary()
    {
        return dictionary;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.BackgroundColor = ConsoleColor.DarkYellow;
        Console.ForegroundColor = ConsoleColor.White;
        string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "bin", "Debug", "net6.0");
       
        Dictionary<string, Dictionaries> existingDictionaries = new Dictionary<string, Dictionaries>();

        Dictionaries dic = new Dictionaries("", "");

        string[] txtFiles = Directory.GetFiles(basePath, "*.txt");

        foreach (string file in txtFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            Dictionaries dictionary = new Dictionaries(null, null);
            dictionary.LoadDictionaryFromFile(file);
            existingDictionaries[fileName] = dictionary;
        }

        string? menu = @"Меню:
1. Создать новый словарь.
2. Добавить слово в существующий словарь.
3. Заменить слово или его перевод в словаре.
4. Найти слово.
5. Слова и перевод(ы) экспортировать в отдельный файл
6. Удалить слово.
7. Удалить перевод слова.
8. Выйти из программы.";

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine(menu);
            Console.Write("Выберите действие: ");
            string choice = Console.ReadLine();
            Console.Clear();
            switch (choice)
            {
                case "1":
                    string? languageFrom, languageTo, filename;
                    Console.Write("С какого языка? ");
                    languageFrom = Console.ReadLine();
                    Console.Write("На какой язык? ");
                    languageTo = Console.ReadLine();
                    Console.Write("Введите имя файла для сохранения словаря: ");
                    filename = Console.ReadLine() + ".txt";

                    Dictionaries newDictionary = new Dictionaries(languageTo, languageFrom);
                    existingDictionaries[filename] = newDictionary;

                    newDictionary.SaveDictionaryToFile(filename);
                    Console.WriteLine($"Создан новый словарь сохранен в файл '{filename}'.");
                    break;

                case "2":
                    if (existingDictionaries.Count > 0)
                    {
                        Console.WriteLine("Существующие словари:");
                        foreach (var dictName in existingDictionaries.Keys)
                        {
                            Console.WriteLine(dictName);
                        }
                        Console.Write("Введите имя словаря: ");
                        string selectedDictionary = Console.ReadLine();
                        if (existingDictionaries.ContainsKey(selectedDictionary))
                        {
                            Dictionaries selectedDict = existingDictionaries[selectedDictionary];
                            Console.Write("Введите слово для добавления: ");
                            string word = Console.ReadLine();
                            Console.Write("Введите перевод(ы) через запятую: ");
                            string[] translations = Console.ReadLine().Split(',');
                            List<string> translationList = new List<string>();
                            foreach (string translation in translations)
                            {
                                translationList.Add(translation.Trim());
                            }
                            selectedDict.AddWord(word, translationList);
                            selectedDict.SaveDictionaryToFile(selectedDictionary + ".txt");
                            Console.WriteLine($"Слово '{word}' успешно добавлено в словарь '{selectedDictionary}'.");
                        }
                        else
                        {
                            Console.WriteLine("Словарь не найден.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Нет существующих словарей.");
                    }
                    Console.ReadLine();
                    Console.Clear();
                    break;

                case "3":
                    Console.WriteLine("Существующие словари:");
                    foreach (var dictName in existingDictionaries.Keys)
                    {
                        Console.WriteLine(dictName);
                    }
                    Console.Write("Введите имя словаря: ");
                    string? selectedDictName = Convert.ToString(Console.ReadLine());
                    Console.WriteLine($"Словарь '{selectedDictName}':");
                    if (existingDictionaries.ContainsKey(selectedDictName))
                    {
                        uint i = 1;
                        Dictionaries words = existingDictionaries[selectedDictName];
                        foreach (var word in existingDictionaries[selectedDictName].GetDictionary())
                        {
                            Console.WriteLine($"{i}. {word.Key}: {string.Join(", ", word.Value)}");
                            i++;
                        }
                        Console.WriteLine();
                        Console.WriteLine("Выберите действие:");
                        Console.WriteLine("1. Заменить слово");
                        Console.WriteLine("2. Заменить перевод слова");
                        Console.Write("Ваш выбор: ");
                        string replaceChoice = Console.ReadLine();
                        switch (replaceChoice)
                        {
                            case "1":
                                Console.Write("Введите слово, которое хотите заменить: ");
                                string oldWord = Console.ReadLine();
                                Console.Write("Введите новое слово: ");
                                string newWord = Console.ReadLine();
                                existingDictionaries[selectedDictName].ReplaceWord(oldWord, newWord, selectedDictName + ".txt");
                                break;
                            case "2":
                                Console.Write("Введите слово: ");
                                string selectedWord = Console.ReadLine();
                                Console.Write("Введите старый перевод: ");
                                string oldTranslation = Console.ReadLine();
                                Console.Write("Введите новый перевод: ");
                                string newTranslation = Console.ReadLine();
                                existingDictionaries[selectedDictName].ReplaceTranslation(selectedWord, oldTranslation, newTranslation, selectedDictName + ".txt");
                                break;
                            default:
                                Console.WriteLine("Неверный ввод.");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Словарь не найден.");
                    }
                    Console.ReadLine();
                    Console.Clear();
                    break;

                case "4":
                    Console.WriteLine("Существующие словари:");
                    foreach (var dictName in existingDictionaries.Keys)
                    {
                        Console.WriteLine(dictName);
                    }
                    Console.Write("Введите имя файла словаря: ");
                    string? selectedFile = Console.ReadLine() + ".txt";
                    Console.Write("Введите искаемое слово: ");
                    string? searchWord = Console.ReadLine();
                    dic.SearchWord(searchWord, selectedFile);
                    Console.ReadLine();
                    Console.Clear();
                    break;

                case "5":
                    Console.WriteLine("Существующие словари:");
                    foreach (var dictName in existingDictionaries.Keys)
                    {
                        Console.WriteLine(dictName);
                    }
                    Console.Write("Введите имя файла для экспорта (.txt): ");
                    string exportFileName = Console.ReadLine() + ".txt";
                    Console.Write("Введите имя существующего словаря для экспорта: ");
                    string selectedFile5 = Console.ReadLine() + ".txt";
                    dic.ExportDictionaryToFile(selectedFile5, exportFileName);
                    Console.ReadLine();
                    Console.Clear();
                    break;

                case "6":
                    Console.WriteLine("Существующие словари:");
                    foreach (var dictName in existingDictionaries.Keys)
                    {
                        Console.WriteLine(dictName);
                    }
                    Console.Write("Введите имя файла словаря: ");
                    string? selectedFile2 = Console.ReadLine() + ".txt";
                    if (File.Exists(selectedFile2))
                    {
                        uint i = 1;
                        Console.WriteLine("Слова в словаре:");
                        string[] words = File.ReadAllLines(selectedFile2);
                        foreach (string item in words)
                        {
                            Console.WriteLine($"{i}. {item}");
                            i++;
                        }

                        Console.Write("Введите удаляемое слово: ");
                        string? removeWord = Console.ReadLine();
                        dic.RemoveWord(removeWord, selectedFile2);
                        Console.ReadLine();
                        Console.Clear();
                    }
                    else
                    {
                        Console.WriteLine("Файл словаря не найден.");
                    }
                    break;


                case "7":
                    Console.WriteLine("Существующие словари:");
                    foreach (var dictName in existingDictionaries.Keys)
                    {
                        Console.WriteLine(dictName);
                    }
                    Console.Write("Введите имя файла словаря: ");
                    string? selectedFile3 = Console.ReadLine() + ".txt";
                    if (File.Exists(selectedFile3))
                    {
                        uint i = 1;
                        Console.WriteLine("Слова в словаре:");
                        string[] words = File.ReadAllLines(selectedFile3);
                        foreach (string item in words)
                        {
                            Console.WriteLine($"{i}. {item}");
                            i++;
                        }
                    }
                    Console.Write("Введите слово: ");
                    string? _removeWord = Console.ReadLine();

                    Console.Write("Введите перевод слова, которую хотите удалить: ");
                    string? removeTranslation = Console.ReadLine();
                    dic.RemoveTranslation(_removeWord, removeTranslation, selectedFile3);
                    Console.ReadLine();
                    Console.Clear();
                    break;

                case "8":
                    exit = true;
                    Console.WriteLine("Пока!  Sayōnara!  GoodBye!  До побачення!");
                    break;

                default:
                    Console.WriteLine("Неверный ввод. Пожалуйста, выберите действие из списка.");
                    break;
            }
        }
    }
}
