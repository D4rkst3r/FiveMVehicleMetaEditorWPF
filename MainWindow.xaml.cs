using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.ViewModels;
using Newtonsoft.Json;

namespace FiveMVehicleMetaEditorWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;
        private readonly string _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FiveMVehicleMetaEditor",
            "window.json");

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            // Load saved window state
            LoadWindowState();
        }

        private void LoadWindowState()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var state = JsonConvert.DeserializeObject<WindowStateInfo>(json);
                    if (state != null)
                    {
                        Width = state.Width;
                        Height = state.Height;
                        Left = state.X;
                        Top = state.Y;
                        if (state.IsMaximized)
                            WindowState = System.Windows.WindowState.Maximized;
                    }
                }
            }
            catch { }
        }

        private void SaveWindowState()
        {
            try
            {
                var state = new WindowStateInfo
                {
                    Width = (int)Width,
                    Height = (int)Height,
                    X = (int)Left,
                    Y = (int)Top,
                    IsMaximized = System.Windows.WindowState.Maximized == this.WindowState
                };

                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
                File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(state, Formatting.Indented));
            }
            catch { }
        }

        private class WindowStateInfo
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public bool IsMaximized { get; set; }
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tabId)
            {
                _viewModel?.ShowTab(tabId);
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            SaveWindowState();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SaveWindowState();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowState();
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveWindowState();
            base.OnClosed(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Check for keyboard shortcuts
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.S:  // Ctrl+S - Save
                        if (_viewModel?.SaveCommand.CanExecute(null) == true)
                        {
                            _viewModel.SaveCommand.Execute(null);
                            ShowNotification("✓ File saved successfully!", 2000);
                            e.Handled = true;
                        }
                        break;

                    case Key.E:  // Ctrl+E - Export
                        if (_viewModel?.ExportCommand.CanExecute(null) == true)
                        {
                            _viewModel.ExportCommand.Execute(null);
                            ShowNotification("📤 File exported!", 2000);
                            e.Handled = true;
                        }
                        break;

                    case Key.I:  // Ctrl+I - Import/Load
                        if (_viewModel?.LoadCommand.CanExecute(null) == true)
                        {
                            _viewModel.LoadCommand.Execute(null);
                            ShowNotification("📂 File loaded!", 2000);
                            e.Handled = true;
                        }
                        break;

                    case Key.F:  // Ctrl+F - Find/Search
                        _viewModel?.ShowTab("browser");
                        ShowNotification("🔍 Search Browser opened", 1500);
                        e.Handled = true;
                        break;
                }
            }
            else if (e.Key == Key.F1)
            {
                // F1 - Show Shortcuts Dialog
                ShowShortcutsDialog();
                e.Handled = true;
            }
        }

        private void ShowShortcutsDialog()
        {
            MessageBox.Show(
                "⌨️ KEYBOARD SHORTCUTS\n\n" +
                "Ctrl+S ......... Save current file\n" +
                "Ctrl+E ......... Export as profile\n" +
                "Ctrl+I ......... Import/Load profile\n" +
                "Ctrl+F ......... Open search browser\n" +
                "F1 ............. Show this dialog\n\n" +
                "📋 NAVIGATION\n" +
                "Click any tab in the left sidebar to switch views\n" +
                "Use mouse scroll to navigate if tabs overflow\n\n" +
                "💾 FILE MANAGEMENT\n" +
                "Auto-saves window size and position\n" +
                "Recent files tracked automatically",
                "FiveM Vehicle Meta Editor - Shortcuts",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ShowNotification(string message, int duration = 3000)
        {
            _viewModel?.SetStatus(message);
            // Auto-clear after duration
            Task.Delay(duration).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => _viewModel?.SetStatus("Ready"));
            });
        }
    }
}