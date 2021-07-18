using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace XR.Std.Net
{
    public static class JsonTools
    {
        public static async Task<string> GetJsonAsync(string url)
        {
            return await GetRaw(url);
        }

        public static async Task<string> GetStringAsync(string url)
        {
            return await GetRaw(url);
        }

        public static async Task<string> PostFormUrlStringAsync(string url, Dictionary<string, string> values)
        {
            return await PostRaw(url, values, true);
        }

        public static async Task<string> PostJsonStringAsync(string url, string json)
        {
            return await PostRaw(url, json, false);
        }

        private static async Task<string> GetRaw(string url)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync();
            contentStream.Position = 0;
            var sr = new StreamReader(contentStream);
            return sr.ReadToEnd();
        }

        private static async Task<string> PostRaw(string url, object content, bool isFormUrl)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (isFormUrl)
            {
                request.Content = new FormUrlEncodedContent(content as Dictionary<string, string>);
            }
            else
            {
                request.Content = new StringContent(content as string);
            }
            using var contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync();
            contentStream.Position = 0;
            var sr = new StreamReader(contentStream);
            return sr.ReadToEnd();
        }

        public static class JSON<TType> where TType : class
        {
            /// <summary>
            /// Serializes an object to JSON
            /// </summary>
            public static string Serialize(TType instance)
            {
                var serializer = new DataContractJsonSerializer(typeof(TType));
                using var stream = new MemoryStream();
                serializer.WriteObject(stream, instance);
                // todo: break with !@#$%¨&*() in default encoding
                return Encoding.UTF8.GetString(stream.ToArray());
            }

            /// <summary>
            /// DeSerializes an object from JSON
            /// </summary>
            public static TType Deserialize(string json)
            {
                // todo: break with !@#$%¨&*() in default encoding
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                var serializer = new DataContractJsonSerializer(typeof(TType));
                return serializer.ReadObject(stream) as TType;
            }
        }
    }
}

