using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using TagsWpf.Interfaces;
using TagsWpf.Services;
using TagsWpf.Views;

namespace TagsWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            var window = Container.Resolve<MainWindow>();
            return window;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ITagsCounter, TagsCounter>();
            containerRegistry.RegisterSingleton<ISiteListProvider, FileSitesListProvider>();
            containerRegistry.RegisterSingleton<ISiteLoader, SiteLoader>();
        }
    }
}
