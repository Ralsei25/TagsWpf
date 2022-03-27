using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using TagsWpf.Dialogs;
using TagsWpf.Interfaces;
using TagsWpf.Models;

namespace TagsWpf.ViewModels
{
    internal class SiteViewModel : BindableBase
    {
        #region Fields
        private Site _site;
        private int _loadProgress;
        private LoadStatus _loadStatus;
        private SolidColorBrush? _color;
        private bool _isHighlighted;
        private ITagsCounter _tagsCounter;
        private ISiteLoader _siteLoader;
        private CancellationTokenSource _cts;
        #endregion


        public SiteViewModel(Site site, ITagsCounter tagsCounter, ISiteLoader siteLoader)
        {
            _site = site;
            _cts = new CancellationTokenSource();
            _tagsCounter = tagsCounter;
            _siteLoader = siteLoader;

            LoadSiteCommand = new DelegateCommand<string>(LoadSiteAsync, (tag) => LoadStatus != LoadStatus.Loading);
            ShowContentCommand = new DelegateCommand(ShowContent, () => Content != null);
        }


        #region Properties
        public SolidColorBrush StatusColor
        {
            get
            {
                if (LoadStatus == LoadStatus.Error)
                    return Brushes.Red;
                else if (LoadStatus == LoadStatus.Canceled)
                    return Brushes.Yellow;
                else
                    return Brushes.LimeGreen;
            }
        }
        public LoadStatus LoadStatus
        {
            get { return _loadStatus; }
            set
            {
                SetProperty(ref _loadStatus, value);
                RaisePropertyChanged("StatusColor");
                LoadSiteCommand.RaiseCanExecuteChanged();
            }
        }
        public int LoadProgress
        {
            get { return _loadProgress; }
            set { SetProperty(ref _loadProgress, value); }
        }
        public int TagsCount
        {
            get { return _site.TagsCount; }
            set
            {
                _site.TagsCount = value;
                RaisePropertyChanged();
            }
        }
        public string Url => _site.Url;
        public string Content
        {
            get { return _site.Content; }
            set
            {
                _site.Content = value;
                ShowContentCommand.RaiseCanExecuteChanged();
            }
        }
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { SetProperty(ref _isHighlighted, value); }
        }
        #endregion


        #region Commands
        public DelegateCommand<string> LoadSiteCommand { get; private set; }
        public DelegateCommand ShowContentCommand { get; private set; }
        #endregion


        #region Methods
        private void ShowContent()
        {
            var contentViewer = new ContentViewer(Content);
            contentViewer.ShowDialog();
        }
        private async void LoadSiteAsync(string tag)
        {
            await UnloadSiteAsync();
            await LoadSiteAsync(_cts.Token);
            await CountTagAsync(tag);
        }
        public async Task IncreaseProgressAsync(int progress)
        {
            await Task.Run(() =>
            {
                int newProgress = LoadProgress + progress;
                if (newProgress > 100)
                    LoadProgress = 100;
                else if (newProgress < 0)
                    LoadProgress = 0;
                else
                    LoadProgress = newProgress;
            });
        }
        public async Task LoadSiteAsync(CancellationToken token)
        {
            try
            {
                LoadStatus = LoadStatus.Loading;
                await Task.Delay(2000);
                token.ThrowIfCancellationRequested();

                Content = await _siteLoader.GetHtmlAsync(Url);
                await IncreaseProgressAsync(50);
                await Task.Delay(2000);
                token.ThrowIfCancellationRequested();

                LoadProgress = 100;
                LoadStatus = LoadStatus.Loaded;
            }
            catch (OperationCanceledException e)
            {
                LoadStatus = LoadStatus.Canceled;
            }
            catch (Exception e)
            {
                LoadProgress = 100;
                LoadStatus = LoadStatus.Error;
            }
        }
        public async Task CountTagAsync(string tag)
        {
            await Task.Run(() =>
            {
                int tagsCount = _tagsCounter.CountTags(Content, tag);
                TagsCount = tagsCount;
                return TagsCount;
            });
        }
        public async Task UnloadSiteAsync()
        {
            await Task.Run(() =>
            {
                TagsCount = 0;
                LoadStatus = LoadStatus.NotLoaded;
                LoadProgress = 0;
                Content = null;
            });
        }
        #endregion

    }
}
