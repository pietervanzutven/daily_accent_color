using System;
using Windows.ApplicationModel.Background;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DailyAccentColor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AccentColor.Helper accentColorHelper = new AccentColor.Helper();

        private async void RefreshColor()
        {
            Color color = await accentColorHelper.GetColor();
            colorView.Background = new Windows.UI.Xaml.Media.SolidColorBrush(color);
            accentColorHelper.SetColor(color);
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                task.Value.Unregister(true);
            }

            BackgroundExecutionManager.RemoveAccess();
            await BackgroundExecutionManager.RequestAccessAsync();
            var builder = new BackgroundTaskBuilder
            {
                Name = "BackgroundTimer",
                TaskEntryPoint = "BackgroundTimer.Timer"
            };
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            builder.Register();

            RefreshColor();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshColor();
        }
    }
}
