using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

//Altered file from the deathrun mod to have similar data: https://github.com/tinyhoot/Deathrun-Remade/blob/main/DeathrunRemade/Objects/JsonTechData.cs
namespace DrillTool
{ 
    // Since these classes are just for serialising we really don't need warnings about "unused" fields.
#pragma warning disable CS0649
    
    /// <summary>
    /// A wrapper class to make JSON serialisation of recipes easier because UWE's classes contain getters/setters
    /// and that doesn't work too well.
    /// </summary>
    [Serializable]
    public class SerialTechData
    {
        [JsonConverter(typeof(BetterStringEnumConverter))]
        public TechType techType;
        public int craftAmount;
        public List<SerialIngredient> ingredients;
        
        public SerialTechData(TechType techType, int craftAmount = 1, List<SerialIngredient> ingredients = null)
        {
            this.techType = techType;
            this.craftAmount = craftAmount;
            this.ingredients = ingredients;
        }
    }

    [Serializable]
    public class SerialIngredient
    {
        [JsonConverter(typeof(BetterStringEnumConverter))]
        public TechType techType;
        public int amount;

        public SerialIngredient(TechType techType, int amount = 1)
        {
            this.techType = techType;
            this.amount = amount;
        }

        public Ingredient ToIngredient()
        {
            return new Ingredient(techType, amount);
        }

        public override string ToString()
        {
            return $"({amount} {techType})";
        }
    }

    [Serializable]
    public class SerialScanData
    {
        public string techType;
        public int amount;
        
        public SerialScanData() { }
        
        public SerialScanData(string techType, int amount)
        {
            this.techType = techType;
            this.amount = amount;
        }
    }

    /// <summary>
    /// The normal StringEnumConverter results in a nullref for some reason so here's an extremely basic replacement.
    /// </summary>
    internal class BetterStringEnumConverter : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!objectType.IsEnum)
                return existingValue;

            return ParseEnum(objectType, reader.Value?.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string name = value.ToString();
            writer.WriteValue(name);
        }

        private static object ParseEnum(Type type, string value)
        {
            List<FieldInfo> source = type.IsEnum ? AccessTools.GetDeclaredFields(type) : throw new ArgumentException("Type argument must be an enum!");
            return source.FirstOrDefault(f => f.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase))?.GetValue(type) ?? source[0].GetValue(type);
        }
    }
}