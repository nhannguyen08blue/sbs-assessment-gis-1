using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using sbs_assessment_gis_1.Models;
using sbs_assessment_gis_1.Extensions;

namespace sbs_assessment_gis_1.Services;
public class JsonDataParserService
{
    private readonly string _jsonFilePath;

    public JsonDataParserService(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;
    }

    // Directly query the JObect for laureates
    public List<Laureate> PrintList(int? beforeYear, string category = "", string motivation = "")
    {
        using var file = File.OpenText(_jsonFilePath);
        using JsonTextReader reader = new JsonTextReader(file);
        JObject obj = (JObject)JToken.ReadFrom(reader);

        var array = from p in obj["prizes"]
                    select new JArray(p)
                    .FirstOrDefault();

        if (array == null) return new List<Laureate>();

        var result = array
            .Where(p => p["laureates"] != null)
            .WhereIf(beforeYear.HasValue, p => (int)p["year"] < beforeYear.Value)
            .WhereIf(!string.IsNullOrEmpty(category), p => (string)p["category"] == category)
            .WhereIf(!string.IsNullOrEmpty(motivation),
                p => (p["laureates"].Select(i => i["motivation"]))
                    .Any(m => m.Value<string>().Contains(motivation, StringComparison.InvariantCultureIgnoreCase)))
            .SelectMany(p => p["laureates"])
            .ToList();

        List<Laureate> laureateItems = new List<Laureate>();
        foreach (var item in result)
        {
            var jObject = JObject.Parse(item.ToString());
            laureateItems.Add(jObject.ToObject<Laureate>());
        }

        Console.WriteLine(JsonConvert.SerializeObject(result));

        return laureateItems;
    }

    // Alternative: Convert JSON data to .NET object then query
    public List<Laureate> PrintListObject(int? beforeYear, string category = "", string motivation = "")
    {
        using var file = File.OpenText(_jsonFilePath);

        JsonSerializer serializer = new JsonSerializer();
        PrizeData data = (PrizeData)serializer.Deserialize(file, typeof(PrizeData));

        if (data == null) return new List<Laureate>();

        var query = data.Prizes.Where(p => p.Laureates != null)
            .WhereIf(beforeYear.HasValue, p => Convert.ToInt32(p.Year) < beforeYear)
            .WhereIf(!string.IsNullOrEmpty(category), p => p.Category == category)
            .WhereIf(!string.IsNullOrEmpty(motivation),
                p => p.Laureates.Exists(l => l.Motivation.Contains(motivation, StringComparison.InvariantCultureIgnoreCase)))
            .Select(p => p.Laureates);

        List<Laureate> laureateItems = new List<Laureate>();
        foreach (var item in query)
        {
            laureateItems.AddRange(item);
        }

        Console.WriteLine(JsonConvert.SerializeObject(laureateItems));

        return laureateItems;
    }
}
