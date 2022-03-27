using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TagsWpf.Models
{
    internal class Site
    {
        private string _url;
        private int _tagsCount;
        private string? _content;

        public Site(string url)
        {
            Url = url;
            TagsCount = 0;
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        public int TagsCount
        {
            get { return _tagsCount; }
            set { _tagsCount = value; }
        }
        public string? Content
        {
            get { return _content; }
            set { _content = value; }
        }
    }
}
