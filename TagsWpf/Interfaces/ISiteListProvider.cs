using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagsWpf.Interfaces
{
    internal interface ISiteListProvider
    {
        string[] GetSitesList(string soureceAddress);
    }
}
