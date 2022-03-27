using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TagsWpf.Interfaces;

namespace TagsWpf.Services
{
    public class TagsCounter : ITagsCounter
    {
        private const string RegexTemplate = @"<([@tag]+)(?=[\s>])(?:[^>=]|='[^']*'|=""[^""]*"" |=[^'""\s]*)*\s?\/?>";
        public int CountTags(string htmlPage, string tag)
        {
            if (htmlPage == null)
            {
                return 0;
            }
            string regex = RegexTemplate.Replace("@tag", tag);
            return Regex.Matches(htmlPage, regex).Count();
        }
    }
}
