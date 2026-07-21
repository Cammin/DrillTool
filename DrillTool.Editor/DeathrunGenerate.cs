using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DrillTool.Editor;

public static class DeathrunGenerate
{
    public static void WriteDefaultJsonFiles()
    {
        Console.WriteLine();
        Console.WriteLine("Writing default recipes and scans to json files...");
        Console.WriteLine();
        foreach (var pair in ConfigFileLoader.DefaultRecipes)
        {
            Console.WriteLine($"{pair.Key} Recipe:\n{string.Join("\n", pair.Value.Select(p => $"{p.techType}:{p.amount}"))}\n");
        }
        foreach (var pair in ConfigFileLoader.DefaultScans)
        {
            var p = pair.Value;
            Console.WriteLine($"{pair.Key} Scan: {p.techType}:{p.amount}");
        }
        
        string jsonRecipes = JsonConvert.SerializeObject(ConfigFileLoader.DefaultRecipes, Formatting.Indented);
        string jsonScans = JsonConvert.SerializeObject(ConfigFileLoader.DefaultScans, Formatting.Indented);
        //write to file!!

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectFolder = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName ?? baseDir;
        string outputFolder = Path.Combine(projectFolder, "Config");
        
        string recipesPath = Path.Combine(outputFolder, ConfigFileLoader.RecipesFileName);
        string scansPath = Path.Combine(outputFolder, ConfigFileLoader.ScansFileName);
        
        File.WriteAllText(recipesPath, jsonRecipes);
        File.WriteAllText(scansPath, jsonScans);

        OpenFolder(projectFolder);
    }
    
    private static void OpenFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine("folder doesnt exist at " + path);
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}