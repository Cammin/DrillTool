using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json; // NuGet: Newtonsoft.Json

namespace DrillTool;

//sheets doc that gets loaded and turned to the json files.
//https://docs.google.com/spreadsheets/d/1DaJR9qEDWyGWqvxv5k4h_7KObrdWGRYjMBmU2DjlGjw/edit?usp=sharing

public static class TranslationGenerator
{
    [UsedImplicitly]
    public static void ProcessTranslations()
    {
        string outputFolder = Directory.GetCurrentDirectory();
        string csvPath = Path.Combine(outputFolder, "translations.csv"); 

        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"No file at {csvPath}");
            return;
        }
        Console.WriteLine($"Reading from {csvPath}");
        
        ProcessTranslations(csvPath, outputFolder);
    }
    
    public static void ProcessTranslations(string csvPath, string outputFolder)
    {
        // Regex to handle CSV cells that contain commas and newlines wrapped in quotes
        var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        string[] lines = File.ReadAllLines(csvPath);

        if (lines.Length < 3) return;

        // Row 1 contains language names (English, Spanish, etc.)
        string[] headers = csvParser.Split(lines[0]);

        // Loop through each language column (Starting at Index 1)
        for (int col = 1; col < headers.Length; col++)
        {
            string language = headers[col].Trim().Replace("\"", "");
            var jsonContent = new Dictionary<string, string>();

            // Loop through data rows (Starting at Index 2 to skip headers/ISO codes)
            for (int row = 2; row < lines.Length; row++)
            {
                string[] fields = csvParser.Split(lines[row]);

                if (fields.Length > col)
                {
                    string key = fields[0].Trim().Replace("\"", "");
                    // Clean up the value: remove surrounding quotes and handle escaped double quotes
                    string value = fields[col].Trim();
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                        value = value.Substring(1, value.Length - 2).Replace("\"\"", "\"");

                    if (!string.IsNullOrEmpty(key))
                    {
                        jsonContent[key] = value;
                    }
                }
            }

            // Save to individual file: e.g., English.json
            string jsonString = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);
            File.WriteAllText(Path.Combine(outputFolder, $"{language}.json"), jsonString);
            
            Console.WriteLine($"Successfully created {language}.json");
        }
    }
}