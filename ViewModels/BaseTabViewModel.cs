using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;

namespace FiveMVehicleMetaEditorWPF.ViewModels
{
    /// <summary>
    /// Base ViewModel for all tabs - provides common functionality
    /// </summary>
    public abstract class BaseTabViewModel : INotifyPropertyChanged
    {
        protected MainWindowViewModel? MainVM { get; set; }

        private string _statusMessage = "";
        private bool _isLoading = false;
        private string _searchText = "";

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                    MainVM?.SetStatus(value);
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    OnSearchChanged(value);
                }
            }
        }

        public ICommand SaveCommand { get; protected set; }
        public ICommand LoadCommand { get; protected set; }
        public ICommand ExportCommand { get; protected set; }

        protected BaseTabViewModel(MainWindowViewModel? mainVM = null)
        {
            MainVM = mainVM;
            SaveCommand = new RelayCommand(_ => OnSave());
            LoadCommand = new RelayCommand(_ => OnLoad());
            ExportCommand = new RelayCommand(_ => OnExport());
        }

        protected virtual void OnSave() { }
        protected virtual void OnLoad() { }
        protected virtual void OnExport() { }
        protected virtual void OnSearchChanged(string searchText) { }

        public void ShowSuccess(string message) => StatusMessage = $"✓ {message}";
        public void ShowError(string message) => StatusMessage = $"✗ {message}";
        public void ShowInfo(string message) => StatusMessage = message;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
