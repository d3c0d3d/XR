using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace XR.Core.Util
{
    public static class Utilities
    {
        public static async Task<string> GetTextFileAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    using (var contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync())
                    {
                        contentStream.Position = 0;
                        var sr = new StreamReader(contentStream);
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
