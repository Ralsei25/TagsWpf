using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagsWpf.Interfaces
{
    internal interface ISiteLoader
    {
        string GetHtml(string url);
        Task<string> GetHtmlAsync(string url);
    }
}
