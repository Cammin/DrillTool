using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json; // NuGet: Newtonsoft.Json

namespace DrillTool;

//sheets doc that gets loaded and turned to the json files.
//https://docs.google.com/spreadsheets/d/1DaJR9qEDWyGWqvxv5k4h_7KObrdWGRYjMBmU2DjlGjw/edit?usp=sharing

public static class LocalizationGenerate
{
    public static void ProcessTranslations()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectFolder = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.FullName ?? baseDir;
        string csvPath = Path.Combine(projectFolder, "translate.csv");
        string outputFolder = Path.Combine(projectFolder, "output");

        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"No file at {csvPath}");
            return;
        }
        Console.WriteLine($"Reading from {csvPath}");
        
        ProcessFile(csvPath, outputFolder);
    }
    
    public static void ProcessFile(string csvPath, string outputFolder)
    {
        // Regex to handle CSV cells that contain commas and newlines wrapped in quotes
        var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        string[] rows = File.ReadAllLines(csvPath);

        Dictionary<string, Dictionary<string, string>> languageDatabase = new Dictionary<string, Dictionary<string, string>>();
        
        // Row 1 contains language names (English, Spanish, etc.)
        string[] languageLabels = csvParser.Split(rows[0]);
        int gridWidth = languageLabels.Length;
        
        for (var i = 1; i < gridWidth; i++)
        {
            languageDatabase.Add(languageLabels[i], new Dictionary<string, string>());
        }
        
        //only deal with one row at a time
        for (int y = 2; y < rows.Length; y++)
        {
            string rowString = rows[y];
            string[] rowElements = csvParser.Split(rowString);
            string key = rowElements[0];
            
            for (int x = 1; x < gridWidth; x++)
            {
                string language = languageLabels[x];
                languageDatabase[language].Add(key, rowElements[x]);
            }
        }

        foreach (var language in languageDatabase)
        {
            string languageLabel = language.Key;
            string jsonContent = JsonConvert.SerializeObject(language.Value, Formatting.Indented);
            File.WriteAllText(Path.Combine(outputFolder, $"{languageLabel}.json"), jsonContent);
            
            Console.WriteLine($"Successfully created {languageLabel}.json");
        }
    }
}