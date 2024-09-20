namespace sbs_assessment_gis_1.Models;

public class PrizeData
{
    public List<Prize> Prizes { get; set; }
}

public class Prize
{
    public string Year { get; set; }
    public string Category { get; set; }
    public string OverallMotivation { get; set; }
    public List<Laureate> Laureates { get; set; }
}

public class Laureate
{
    public string Id { get; set; }
    public string Firstname { get; set; }
    public string Surname { get; set; }
    public string Motivation { get; set; }
    public string Share { get; set; }
}
