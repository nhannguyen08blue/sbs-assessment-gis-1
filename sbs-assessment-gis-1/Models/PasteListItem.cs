using sbs_assessment_gis_1.Models.Enums;
using System.Xml.Linq;

namespace sbs_assessment_gis_1.Models;

public class PasteListItem
{
    public string Key { get; set; }

    public DateTime Date { get; set; }

    public string Title { get; set; }

    public int Size { get; set; }

    public DateTime ExpireDate { get; set; }

    public PasteBinPrivacy Private { get; set; }

    public string FormatLong { get; set; }

    public string FormatShort { get; set; }

    public string Url { get; set; }

    public int Hits { get; set; }

    public static PasteListItem FromXML(XElement xElement)
    {
        var paste = new PasteListItem();

        paste.Key = xElement.Element("paste_key").Value;
        paste.Date = DateTimeOffset.FromUnixTimeSeconds((long)xElement.Element("paste_date")).UtcDateTime;
        paste.Title = xElement.Element("paste_title").Value;
        paste.Size = (int)xElement.Element("paste_size");
        var expireDate = (long)xElement.Element("paste_expire_date");
        paste.ExpireDate = expireDate != 0 ? DateTimeOffset.FromUnixTimeSeconds(expireDate).UtcDateTime : paste.Date;
        paste.Private = (PasteBinPrivacy)(int)xElement.Element("paste_private");
        paste.FormatLong = xElement.Element("paste_format_long").Value;
        paste.FormatShort = xElement.Element("paste_format_short").Value;
        paste.Url = xElement.Element("paste_url").Value;
        paste.Hits = (int)xElement.Element("paste_hits");

        return paste;
    }

    public static IEnumerable<PasteListItem> PastesFromXML(string xml)
    {
        foreach (var paste in XElement.Parse("<pastes>" + xml + "</pastes>").Descendants("paste"))
            yield return PasteListItem.FromXML(paste);
    }
}
