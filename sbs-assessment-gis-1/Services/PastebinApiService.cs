using Newtonsoft.Json;
using sbs_assessment_gis_1.Models;
using sbs_assessment_gis_1.Models.Enums;

namespace sbs_assessment_gis_1.Services;

public class PastebinApiService
{
    private readonly HttpClient _client;
    private readonly PastebinOptions _options;
    private readonly string _apiDevKey;

    public PastebinApiService(HttpClient client, PastebinOptions options, string apiDevKey)
    {
        _client = client;
        _options = options;
        _apiDevKey = apiDevKey;
    }

    /// <summary>
    /// Needed to login in order to delete paste as guest paste isn't removable.
    /// Per pastebin's recommendation, the key should be cached/stored once created
    /// as it does not expire.
    /// </summary>
    /// <param name="username">Pastebin username</param>
    /// <param name="password">Pastebin password</param>
    /// <returns>Pastebin User token string</returns>
    public async Task<string> LoginPaste(string username, string password)
    {
        var body = new Dictionary<string, string>()
            {
                { "api_dev_key", _apiDevKey },
                { "api_user_name", username },
                { "api_user_password", password }
            };

        return await SendFormURIRequest(HttpMethod.Post, _options.LoginURI, body);
    }

    /// <summary>
    /// Create a json paste and return the paste's key
    /// </summary>
    /// <param name="pasteName">Name of paste</param>
    /// <param name="pasteBody">Paste content</param>
    /// <param name="privacy">Visibility of paste (0 - Public, 1 - Unlisted, 2 - Private)</param>
    /// <param name="apiUserKey">API User Key</param>
    /// <returns>Paste unique key string</returns>
    public async Task<string> CreatePaste(string pasteName, object pasteBody, PasteBinPrivacyEnum privacy, PasteBinExpirationEnum expiration, string apiUserKey)
    {
        var body = new Dictionary<string, string>()
        {
            { "api_dev_key", _apiDevKey },
            { "api_user_key", apiUserKey },
            { "api_paste_code", JsonConvert.SerializeObject(pasteBody) },
            { "api_paste_expire_date", FormatExpirationDateInput(expiration) },
            { "api_paste_private", privacy.ToString("D") },
            { "api_paste_format", "json" },
            { "api_paste_name", pasteName },
            { "api_option", "paste" }
        };

        return await SendFormURIRequest(HttpMethod.Post, _options.CreateURI, body);
    }

    public async Task<string> ListPaste(string apiUserKey, int resultLimit = 100)
    {
        var body = new Dictionary<string, string>()
        {
            { "api_dev_key", _apiDevKey },
            { "api_user_key", apiUserKey },
            { "api_results_limit", resultLimit.ToString() },
            { "api_option", "list" }
        };

        return await SendFormURIRequest(HttpMethod.Post, _options.ListURI, body);
    }

    /// <summary>
    /// Delete a paste created by the user with the specified paste key
    /// </summary>
    /// <param name="pasteKey">Paste Key to delete</param>
    /// <param name="apiUserKey">API User key</param>
    /// <returns>Success message</returns>
    public async Task<string> DeletePaste(string pasteKey, string apiUserKey)
    {
        var body = new Dictionary<string, string>()
        {
            { "api_dev_key", _apiDevKey },
            { "api_user_key", apiUserKey },
            { "api_paste_key", pasteKey },
            { "api_option", "delete" }
        };

        return await SendFormURIRequest(HttpMethod.Post, _options.DeleteURI, body);
    }

    // Helper method to send form-url-encoded http requests
    private async Task<string> SendFormURIRequest(HttpMethod method, string uri, Dictionary<string, string> parameters)
    {
        using var request = new HttpRequestMessage(method, uri);

        request.Content = new FormUrlEncodedContent(parameters);

        using var res = await _client.SendAsync(request);
        if (!res.IsSuccessStatusCode)
        {
            // Print out error response from pastebin
            Console.WriteLine($"Request to {uri} failed");
            Console.WriteLine(await res.Content.ReadAsStringAsync());
            return "";
        }

        return await res.Content.ReadAsStringAsync();
    }

    private static string FormatExpirationDateInput(PasteBinExpirationEnum expiration)
    {
        return expiration switch
        {
            PasteBinExpirationEnum.TenMinutes => "10M",
            PasteBinExpirationEnum.OneHour => "1H",
            PasteBinExpirationEnum.OneDay => "1D",
            PasteBinExpirationEnum.OneWeek => "1W",
            PasteBinExpirationEnum.TwoWeeks => "2W",
            PasteBinExpirationEnum.OneMonth => "1M",
            PasteBinExpirationEnum.SixMonths => "6M",
            PasteBinExpirationEnum.OneYear => "1Y",
            _ => "N",
        };
    }
}
