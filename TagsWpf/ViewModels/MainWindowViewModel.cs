using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using TagsWpf;
using TagsWpf.Dialogs;
using TagsWpf.Interfaces;
using TagsWpf.Models;

namespace TagsWpf.ViewModels
{
    class MainWindowViewModel : BindableBase
    {
        #region Fields
        private string _searchTag;
        private string _urlsFilePath;
        private ObservableCollection<SiteViewModel> _sitesList;
        private SiteViewModel _selectedSite;
        private TotalLoadStatus? _allLoadStatus;
        private bool _isFasterEnabled;
        private bool _isSelectFileEnabled;
        private bool _isLoadSitesEnabled;
        private CancellationTokenSource _cts;
        private ITagsCounter _tagsCounter;
        private ISiteListProvider _siteListProvider;
        private ISiteLoader _siteLoader;
        #endregion


        public MainWindowViewModel(ITagsCounter tagsCounter, ISiteListProvider siteListProvider, ISiteLoader siteLoader)
        {
            SearchTag = "a";
            _tagsCounter = tagsCounter;
            _siteListProvider = siteListProvider;
            _siteLoader = siteLoader;
            _cts = new CancellationTokenSource();

            SitesList = new ObservableCollection<SiteViewModel>();
            SitesList.CollectionChanged += OnSitesCollectionChanged;

            LoadSitesListCommand = new DelegateCommand(LoadSitesList, () => IsLoadSiteListEnabled);
            LoadAllSitesCommand = new DelegateCommand(LoadAllSitesAsync, () => IsLoadSitesEnabled);
            FasterCommand = new DelegateCommand(Faster, () => IsFasterEnabled);
            CancelCommand = new DelegateCommand(Cancel);
            AddSiteCommand = new DelegateCommand(AddSite);
            RemoveSelectedSiteCommand = new DelegateCommand(RemoveSelectedSite, () => SelectedSite != null);

            IsFasterEnabled = false;
            IsLoadSitesEnabled = false;
            IsLoadSiteListEnabled = true;
        }


        #region Properties
        /// <summary>
        /// Цвет итогового статус бара
        /// </summary>
        public SolidColorBrush AllStatusColor
        {
            get
            {
                SolidColorBrush result = Brushes.LimeGreen;
                if (SitesList.Count > 1)
                {
                    if (!SitesList.Any(x => x.LoadStatus != LoadStatus.Loaded)) // Все Loaded
                        result = Brushes.LimeGreen;
                    else if (!SitesList.Any(x => x.LoadStatus != LoadStatus.Error)) // Все Error
                        result = Brushes.Red;
                    else if (SitesList.Any(x => x.LoadStatus == LoadStatus.Loaded) && SitesList.Any(x => x.LoadStatus == LoadStatus.Error))
                        result = Brushes.Gold;
                }
                return result;
            }
        }
        /// <summary>
        /// Счётчит итогового статус бара
        /// </summary>
        public int AllLoadProgress
        {
            get
            {
                int result = 0;
                if (SitesList.Count > 1)
                {
                    foreach (var siteInfo in SitesList)
                        result += siteInfo.LoadProgress;
                    result /= SitesList.Count;
                }
                return result;
            }
        }
        /// <summary>
        /// Итоговое количество тегов
        /// </summary>
        public int TagsAmount
        {
            get
            {
                int result = 0;
                foreach (var siteInfo in SitesList)
                    result += siteInfo.TagsCount;
                return result;
            }
        }
        /// <summary>
        /// Искомый тег
        /// </summary>
        public string SearchTag
        {
            get { return _searchTag; }
            set { SetProperty(ref _searchTag, value); }
        }
        /// <summary>
        /// Путь к файлу с адресами сайтов
        /// </summary>
        public string UrlsFilePath
        {
            get { return _urlsFilePath; }
            set { SetProperty(ref _urlsFilePath, value); }
        }
        /// <summary>
        /// Коллекция всех сайтов
        /// </summary>
        public ObservableCollection<SiteViewModel> SitesList
        {
            get { return _sitesList; }
            set { SetProperty(ref _sitesList, value); }
        }
        /// <summary>
        /// Выбранные сайт
        /// </summary>
        public SiteViewModel SelectedSite
        {
            get { return _selectedSite; }
            set 
            { 
                SetProperty(ref _selectedSite, value); 
                RemoveSelectedSiteCommand.RaiseCanExecuteChanged();
            }
        }
        /// <summary>
        /// Итоговый статус загрузки
        /// </summary>
        public TotalLoadStatus? AllLoadStatus
        {
            get { return _allLoadStatus; }
            set { SetProperty(ref _allLoadStatus, value); }
        }
        /// <summary>
        /// Разрешено ли ускорение загрузки
        /// </summary>
        public bool IsFasterEnabled
        {
            get { return _isFasterEnabled; }
            set
            {
                SetProperty(ref _isFasterEnabled, value);
                FasterCommand.RaiseCanExecuteChanged();
            }
        }
        /// <summary>
        /// Разрешен ли выбор файла
        /// </summary>
        public bool IsLoadSiteListEnabled
        {
            get { return _isSelectFileEnabled; }
            set
            {
                SetProperty(ref _isSelectFileEnabled, value);
                LoadAllSitesCommand.RaiseCanExecuteChanged();
            }
        }
        /// <summary>
        /// Разрешена ли загрузка сайтов
        /// </summary>
        public bool IsLoadSitesEnabled
        {
            get { return _isLoadSitesEnabled; }
            set
            {
                SetProperty(ref _isLoadSitesEnabled, value);
                LoadAllSitesCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region Commands
        public DelegateCommand LoadSitesListCommand { get; private set; }
        public DelegateCommand LoadAllSitesCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand FasterCommand { get; private set; }
        public DelegateCommand AddSiteCommand { get; private set; }
        public DelegateCommand RemoveSelectedSiteCommand { get; private set; }
        #endregion


        #region Handlers
        private void OnSitePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsHighlighted")
            {
                UpdateTotals();
            }
        }
        private void OnSitesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            IsLoadSitesEnabled = SitesList.Count > 0;
            UpdateTotals();
        }
        #endregion


        #region Methods
        private void LoadSitesList()
        {

            var newFilePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                newFilePath = openFileDialog.FileName;
            }

            if (newFilePath != String.Empty)
            {
                UrlsFilePath = newFilePath;
                SitesList.Clear();
                var sitesList = new List<SiteViewModel>();
                var urlList = _siteListProvider.GetSitesList(UrlsFilePath);
                foreach (var url in urlList)
                {
                    AddSite(url);
                }
            }
        }
        private void AddSite()
        {
            var dialog = new InputBox("Введите адрес сайта");
            if (dialog.ShowDialog() == true)
            {
                AddSite(dialog.ResponseText);
            }
        }
        private void AddSite(string url)
        {
            var site = new Site(url);
            var siteViewModel = new SiteViewModel(site, _tagsCounter, _siteLoader);
            siteViewModel.PropertyChanged += OnSitePropertyChanged;
            SitesList.Add(siteViewModel);
        }
        private void RemoveSelectedSite()
        {
            RemoveSite(SelectedSite);
        }
        private void RemoveSite(SiteViewModel site)
        {
            site.PropertyChanged -= OnSitePropertyChanged;
            SitesList.Remove(site);
        }
        /// <summary>
        /// Обновить итоговые значения
        /// </summary>
        private void UpdateTotals()
        {
            RaisePropertyChanged("TagsAmount");
            RaisePropertyChanged("AllLoadStatus");
            RaisePropertyChanged("AllLoadProgress");
            RaisePropertyChanged("AllStatusColor");
            UpdateTotalStatus();
            HighlightMax();
        }
        /// <summary>
        /// Загрузить все сайты
        /// </summary>
        private async void LoadAllSitesAsync()
        {
            ResetCancellationTokenSource();
            await UnloadAllSitesAsync();
            await LoadAllSitesAsync(_cts.Token);
        }
        private async Task LoadAllSitesAsync(CancellationToken token)
        {
            try
            {
                IsLoadSiteListEnabled = false;
                IsFasterEnabled = true;
                IsLoadSitesEnabled = false;

                var tasksList = new List<Task>();
                Parallel.ForEach(SitesList, (site) =>
                {
                    tasksList.Add(site.LoadSiteAsync(_cts.Token).ContinueWith((task) => site.CountTagAsync(SearchTag),_cts.Token));
                });
                token.ThrowIfCancellationRequested();
                await Task.Run(() => Task.WhenAll(tasksList), _cts.Token);
                token.ThrowIfCancellationRequested();
                HighlightMax();
            }
            catch (OperationCanceledException e)
            {
                foreach (var site in SitesList)
                {
                    if (site.LoadProgress < 100)
                    {
                        site.LoadStatus = LoadStatus.Canceled;
                        site.LoadProgress = 100;
                    }
                }
            }
            finally
            {
                IsLoadSiteListEnabled = true;
                IsFasterEnabled = false;
                IsLoadSitesEnabled = true;
            }
        }
        /// <summary>
        /// Выгрузить все сайты
        /// </summary>
        private async Task UnloadAllSitesAsync()
        {
            var tasksList = new List<Task>();
            Parallel.ForEach(SitesList, (site) =>
            {
                tasksList.Add(site.UnloadSiteAsync());
            });
            await Task.WhenAll(tasksList);
        }
        /// <summary>
        /// Отмена
        /// </summary>
        private void Cancel()
        {
            _cts.Cancel();
        }
        private void ResetCancellationTokenSource()
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }
        /// <summary>
        /// Грузить быстрее!!!!
        /// </summary>
        private void Faster()
        {
            foreach (var siteData in SitesList)
            {
                if (siteData.LoadProgress < 90)
                    siteData.IncreaseProgressAsync(1);
            }
        }
        /// <summary>
        /// Подстветка самого большого количаства тегов
        /// </summary>
        private void HighlightMax()
        {
            if (SitesList.Count > 0)
            {
                SiteViewModel max = SitesList[0];
                foreach (var siteData in SitesList)
                {
                    siteData.IsHighlighted = false;
                    if (max.TagsCount < siteData.TagsCount)
                        max = siteData;
                }
                if (max.TagsCount > 0)
                {
                    max.IsHighlighted = true;
                }
            }
        }
        /// <summary>
        /// Обновить итоговый статус по статусам сайтов
        /// </summary>
        private void UpdateTotalStatus()
        {
            TotalLoadStatus? result = null;
            if (SitesList.Count > 1)
            {
                if (!SitesList.Any(x => x.LoadStatus != LoadStatus.Loading)) // Все Loaded
                    result = TotalLoadStatus.Loading;
                else if (!SitesList.Any(x => x.LoadStatus != LoadStatus.Loaded)) // Все Loaded
                    result = TotalLoadStatus.AllLoaded;
                else if (!SitesList.Any(x => x.LoadStatus != LoadStatus.Error)) // Все Error
                    result = TotalLoadStatus.AllErrors;
                else if (SitesList.Any(x => x.LoadStatus == LoadStatus.Loaded) && SitesList.Any(x => x.LoadStatus == LoadStatus.Error))
                    result = TotalLoadStatus.HasErrors;
                else if (SitesList.Any(x => x.LoadStatus == LoadStatus.Loaded) && SitesList.Any(x => x.LoadStatus == LoadStatus.NotLoaded))
                    result = TotalLoadStatus.HasNotLoaded;
                else if (SitesList.Any(x => x.LoadStatus == LoadStatus.Loaded) && SitesList.Any(x => x.LoadStatus == LoadStatus.NotLoaded))
                    result = TotalLoadStatus.HasNotLoaded;
            }
            AllLoadStatus = result;
        }
        #endregion
    }
}
