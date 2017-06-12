using System;
using System.IO;
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
        //Made to be used with the Digit-Eyes UPC API
        public static class APIInformation
        {
            private const string apiUrl = "https://www.digit-eyes.com/gtin/v2_0/?";
            private const string upcBuffer = "upcCode=";
            private const string fieldNameBuffer = "&field_names=";
            private const string languageBuffer = "&language=";
            private const string appKeyBuffer = "&app_key=";
            private const string sigBuffer = "&signature=";
            private const string keyFileExtension = ".api";

            private static string appKey;
            private static string authKey;
            private static string language = "en";
            private static string fieldName = "description";
            private static bool _initialized = false;

            public static char lineBreakDelimiter = ',';
            public static char lineSeperatorDetlimiter = ':';
            public static string lineTag = "description";

            public static bool Initialized { get => _initialized; }

            public static void Init()
            {
                var di = new DirectoryInfo(Directory.GetCurrentDirectory());
                try
                {
                    string firstFile = $"\\{di.EnumerateFiles().Select(v => v.Name).FirstOrDefault(v => v.EndsWith(keyFileExtension))}";
                    string fullPath = di.FullName + firstFile;
                    string[] contents = File.ReadAllLines(fullPath);
                    appKey = contents[0];
                    authKey = contents[1];
                }
                catch (Exception e)
                {
                    Console.WriteLine($"EasyInv: {e.Message}\nMake sure there is a valid '.api' file in the root. Try 'EasyInv -setup' for more information.");
                    return;
                }
                _initialized = true;
            }

            public static string GetFullUrl(long upcCode)
            {
                string fullUrl = $"{apiUrl}{upcBuffer}{upcCode}{fieldNameBuffer}{fieldName}" +
                    $"{languageBuffer}{language}{appKeyBuffer}{appKey}" +
                    $"{sigBuffer}{SigningSignature(upcCode.ToString(), authKey)}";
                return fullUrl;
            }
        }

        private static HttpClient client;
        private static long currentUpcCode;

        public static string GetItemInformation(long upcCode)
        {
            client = new HttpClient();
            currentUpcCode = upcCode;
            var request = RunAsync();
            request.Wait();
            return request.Result;
        }

        private static async Task<string> RunAsync()
        {
            client.BaseAddress = new Uri("http://localhost:55268/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string item = string.Empty;
            try
            {
                string url = APIInformation.GetFullUrl(currentUpcCode);
                item = await GetInventoryItemAsync(url);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: Something went wrong. {e.Message}");
            }
            return item;
        }

        private static async Task<string> GetInventoryItemAsync(string path)
        {
            string itemContents = "NULL";
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                itemContents = await response.Content.ReadAsStringAsync();
                itemContents = GetTitleFromJSON(itemContents);
                Console.WriteLine($"EasyInv: UPC ({currentUpcCode}) succcesfully read.");
            }
            else
            {
                Console.WriteLine($"EasyInv: Could not retrieve object with UPC ({currentUpcCode}).");
            }
            return itemContents;
        }

        private static string SigningSignature(string upcCode, string auth)
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(auth));
            var m = hmac.ComputeHash(Encoding.UTF8.GetBytes(upcCode));
            return Convert.ToBase64String(m);
        }

        private static string GetTitleFromJSON(string json)
        {
            string title = json.Replace("\"", "").Replace("}", "").Replace("{", "").Split(APIInformation.lineBreakDelimiter).SingleOrDefault(s => s.Contains(APIInformation.lineTag)).Replace("\n", "");
            title = title.Substring(title.IndexOf(APIInformation.lineSeperatorDetlimiter)).Replace(APIInformation.lineSeperatorDetlimiter, ' ').Trim();
            return title;
        }
    }
}
