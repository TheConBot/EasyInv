using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace EasyInv
{
    public static class InventoryAssistant
    {
        private struct APIInformation
        {
            public const string upcBuffer = "upcCode=";
            public const string sigBuffer = "&signature=";

            public const string fieldName = "&field_names=description";
            public const string language = "&language=en";
            public const string appKey = "&app_key=//y3i+dH0mmg";
            public const string authKey = "Yh28K7l2a5Kg4Zv1";
            public const string apiUrl = "https://www.digit-eyes.com/gtin/v2_0/?";

            public const char lineBreakDelimiter = ',';
            public const char lineSeperatorDetlimiter = ':';
            public const string lineTag = "description";
        }

        private static HttpClient client;
        private static string itemTitle;

        private static void SetNewItem(string item)
        {
            itemTitle = item;
        }

        public static void GetItemInformation(long upc, out string info)
        {
            client = new HttpClient();
            RunAsync(upc).Wait();
            info = itemTitle;
        }

        private static async Task RunAsync(long upc)
        {
            client.BaseAddress = new Uri("http://localhost:55268/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                string url = $"{APIInformation.apiUrl}{APIInformation.upcBuffer}{upc.ToString()}{APIInformation.fieldName}{APIInformation.language}{APIInformation.appKey}{APIInformation.sigBuffer}{SigningSignature(upc.ToString(), APIInformation.authKey)}";

                string item = await GetInventoryItemAsync(url, upc);
                SetNewItem(item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task<string> GetInventoryItemAsync(string path, long upc)
        {
            string itemContents = "NULL";
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                itemContents = await response.Content.ReadAsStringAsync();
                itemContents = GetTitleFromJSON(itemContents);
            }
            else
            {
                Console.WriteLine($"WARNING: Could not retrieve object with UPC ({upc}).");
            }
            return itemContents;
        }

        private static string SigningSignature(string upc, string auth)
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(auth));
            var m = hmac.ComputeHash(Encoding.UTF8.GetBytes(upc));
            return Convert.ToBase64String(m);
        }

        private static string GetTitleFromJSON(string json)
        {
            string title = json.Replace("\"", "").Replace("}", "").Replace("{", "").Split(APIInformation.lineBreakDelimiter).Single(s => s.Contains(APIInformation.lineTag)).Replace("\n", "");
            title = title.Substring(title.IndexOf(APIInformation.lineSeperatorDetlimiter)).Replace(APIInformation.lineSeperatorDetlimiter, ' ').Trim();
            return title;
        }
    }
}
