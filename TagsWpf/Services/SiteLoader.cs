using System;
using System.Net.Http;
using System.Threading.Tasks;
using TagsWpf.Interfaces;

namespace TagsWpf.Services
{
    public class SiteLoader : ISiteLoader
    {
        public async Task<string> GetHtmlAsync(string url)
        {
            string html = "";
            using (var client = HttpClientFactory.Create())
            {
                using (HttpResponseMessage response = await client.GetAsync(new UriBuilder(url).Uri))
                {
                    using (HttpContent content = response.Content)
                    {
                        html = await content.ReadAsStringAsync();
                    }
                }
            }
            return html;
        }

        public string GetHtml(string url)
        {
            return GetHtmlAsync(url).Result;
        }

    }
}
