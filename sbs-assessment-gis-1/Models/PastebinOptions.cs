namespace sbs_assessment_gis_1.Models;

public class PastebinOptions
{
    public string LoginURI { get; set; } = "https://pastebin.com/api/api_login.php";

    public string CreateURI { get; set; } = "https://pastebin.com/api/api_post.php";

    public string ListURI { get; set; } = "https://pastebin.com/api/api_post.php";

    public string DeleteURI { get; set; } = "https://pastebin.com/api/api_post.php";
}
