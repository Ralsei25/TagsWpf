using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagsWpf.Models
{
    internal enum TotalLoadStatus
    {
        Default,
        [Description("Не загружено")]
        NotLoaded,
        [Description("Загружается....")]
        Loading,
        [Description("Всё загружено")]
        AllLoaded,
        [Description("Все ошибки")]
        AllErrors,
        [Description("Есть ошибки")]
        HasErrors,
        [Description("Есть не загруженные")]
        HasNotLoaded,
        [Description("Отменено")]
        Canceled
    }
}
