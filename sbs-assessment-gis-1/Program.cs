using sbs_assessment_gis_1.Models;
using sbs_assessment_gis_1.Models.Enums;
using sbs_assessment_gis_1.Services;

namespace sbs_assessment_gis_1
{
    static class Program
    {
        static readonly HttpClient client = new HttpClient();

        // Simplistic reuse of user key once obtained. Should be stored in cache or DB per user in reality.
        static string api_user_key = "";

        static async Task Main(string[] args)
        {
            // This assessment will involve a console application to get a list of nobel laureates
            // from a local json store and create a pastebin.

            // - The first step is to write a method that will return the list AND print (Console.WriteLine)
            //   the list of laureates based on criteria that might change.

            //   For example, the criteria could be to print all laureates based on category, year, or even for something in the 'motivation'.

            // - For this exercise, we want to get a list of laureates in medicine before the year 1980.


            // - The second step is create a method to leverage the pastebin api (https://pastebin.com/doc_api#1) to create a pastebin
            //   where the contents of the pastebin will be the report.

            // - Finally, create a method that will allow us to delete the pastebin.

            // Note: The API Key will have been sent out via email.
            // Note: Newtonsoft is included in the nuget packages for convenience, but it is optional
            //       and you may use any other library of your choosing.

            // Getting api key from launch parameters. Ideally, it should be read from local secret store or environment variable instead.
            if (args.Length == 0)
            {
                Console.WriteLine("No Pastebin API key found in argument. Please try again.");
                Console.WriteLine("Example: ./sbs-assessment-gis-1 82kdjasoifjasf_1asfuhgas098");
                Console.Read();
                return;
            }

            string api_dev_key = args[0];

            // Normally, DI would be used to inject the service implementations
            JsonDataParserService jsonService = new JsonDataParserService("data.json");
            PastebinApiService pasteService = new PastebinApiService(client, new PastebinOptions(), api_dev_key);

            // Print list of laureates by specified criteria to console, also storing the items to create a paste with it
            // Query through JObject
            var laureates = jsonService.PrintList(1985, "medicine");
            // Alternatively: Parse to .NET object then query (using PrintListObject method)

            // Login to pastebin first
            // For simplicity, prompt the user to input their pastebin credentials here.
            // If there's still no user key (due to invalid credentials or server downtime), creating a
            // paste is still possible by submitting as guest. Deleting it, however, won't be possible.
            // For this exercise, the user is prevented from creating a paste until they're logged in.
            Console.WriteLine();
            while (string.IsNullOrEmpty(api_user_key))
            {
                var credentials = InputCredentials();
                api_user_key = await pasteService.LoginPaste(credentials.username, credentials.password);
            }

            var pasteUrl = await pasteService.CreatePaste("Laureates", laureates, PasteBinPrivacy.Unlisted, PasteBinExpiration.Never, api_user_key);
            Console.WriteLine($"Paste created. Url: {pasteUrl}");

            // Delete the paste if the user exists and the paste's been created
            if (!string.IsNullOrEmpty(api_user_key) && !string.IsNullOrEmpty(pasteUrl))
            {
                // List pastes created by user (so we can actually get the key as the create endpoint does not return it)
                var pastesList = await pasteService.ListPaste(api_user_key);
                if (!string.IsNullOrEmpty(pastesList))
                {
                    // Parse from XML returned by Pastebin
                    var pastes = PasteListItem.PastesFromXML(pastesList);

                    // Find the paste we've just created using the url, then delete it if exists
                    var pasteKey = pastes.FirstOrDefault(p => p.Url == pasteUrl);
                    if (pasteKey != null)
                    {
                        var delMessage = await pasteService.DeletePaste(pasteKey.Key, api_user_key);
                        if (delMessage == "Paste Removed")
                        {
                            Console.WriteLine($"Paste {pasteKey.Key} deleted.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed to find paste to delete.");
                }
            }

            Console.Read();
        }

        // Quick helper method to obtain credentials from console input
        public static (string username, string password) InputCredentials()
        {
            Console.WriteLine("Please input your pastebin username: ");
            string username = Console.ReadLine();
            while (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Username can't be empty.");
                username = Console.ReadLine();
            }
            Console.WriteLine("Password: ");
            string password = Console.ReadLine();
            while (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Password can't be empty.");
                password = Console.ReadLine();
            }

            return (username, password);
        }
    }
}
