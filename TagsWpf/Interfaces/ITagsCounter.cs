using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagsWpf.Interfaces
{
    internal interface ITagsCounter
    {
        int CountTags(string htmlPage, string tag);
    }
}
