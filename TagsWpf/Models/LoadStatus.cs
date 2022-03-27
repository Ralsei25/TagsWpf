using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagsWpf.Models
{
    internal enum LoadStatus
    {
        Default,
        [Description("Не загружено")]
        NotLoaded,
        [Description("Загружается....")]
        Loading,
        [Description("Загружено")]
        Loaded,
        [Description("Ошибка")]
        Error,
        [Description("Отменено")]
        Canceled
    }
}
