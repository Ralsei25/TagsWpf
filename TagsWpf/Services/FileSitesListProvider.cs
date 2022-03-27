using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TagsWpf.Interfaces;

namespace TagsWpf.Services
{
    public class FileSitesListProvider : ISiteListProvider
    {
        public string[] GetSitesList(string soureceAddress)
        {
            try
            {
                var content = File.ReadAllText(soureceAddress).Split("\r\n");
                return content;
            }
            catch
            {
                MessageBox.Show("Ошибка. Похоже мы не можем открыть файл.\nПроверьте что у Вас есть права на его чтение и что он не занят другой программой");
                return new string[0];
            }
        }
    }
}
