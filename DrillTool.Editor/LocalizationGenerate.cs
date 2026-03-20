using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using MiniExcelLibs;

namespace DrillTool.Editor;

//sheets doc that exports the xlsx file and turned into json files.
//https://docs.google.com/spreadsheets/d/1DaJR9qEDWyGWqvxv5k4h_7KObrdWGRYjMBmU2DjlGjw/edit?usp=sharing

public static class LocalizationGenerate
{
    public static void ProcessTranslations()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectFolder = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName ?? baseDir;
        string xlsxPath = Path.Combine(projectFolder, "translate.xlsx");
        string outputFolder = Path.Combine(projectFolder, "Localization");

        
        if (!File.Exists(xlsxPath))
        {
            Console.WriteLine($"No file at {xlsxPath}");
            return;
        }

        Console.WriteLine($"Reading from {xlsxPath}");
        
        Directory.CreateDirectory(outputFolder);
        
        var rows = MiniExcel.Query(xlsxPath);
        var cells = new List<List<string>>();
        foreach (IDictionary<string, object> row in rows)
        {
            //check if we should terminate because we've reached the bottom edge
            if (row.Values.First() == null)
            {
                break;
            }
            
            var line = new List<string>();
            foreach (var cell in row.Values)
            {
                //check if we should terminate because we've reached the right edge
                if (cell == null)
                {
                    break;
                }

                line.Add(Convert.ToString(cell).Trim());
            }

            cells.Add(line);
        }
        
        int yMin = 0;
        int yMax = cells.Count;
        int xMin = 0;
        int xMax = cells.First().Count;

        Console.WriteLine($"{yMin},{yMax}, {xMin},{xMax}");
        
        //each x is a language. gonna get everything in the column
        for (int x = 1; x < xMax; x++)
        {
            string languageLabel = cells[yMin][x];
            if (string.IsNullOrWhiteSpace(languageLabel))
            {
                Console.WriteLine($"Warning: cell at {x} was empty for languageLabel");
                continue;
            }
            Console.WriteLine($"starting languageLabel for {languageLabel} at {x}");

            Dictionary<string, string> translations = new Dictionary<string, string>();

            //get everything for the one language in the current column
            for (int y = 2; y < yMax; y++)
            {
                string key = cells[y][xMin];
                string value = cells[y][x];

                if (string.IsNullOrWhiteSpace(key))
                {
                    Console.WriteLine($"Warning: {languageLabel} cell at {x}_{y} was empty, it should be filled");
                    continue;
                }

                translations[key] = value;
            }

            string jsonContent = JsonConvert.SerializeObject(translations, Formatting.Indented);
            
            File.WriteAllText(Path.Combine(outputFolder, $"{languageLabel}.json"), jsonContent);

            Console.WriteLine($"Successfully created {languageLabel}.json");
        }
    }
}