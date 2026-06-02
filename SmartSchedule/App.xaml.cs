using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace SmartSchedule
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Prevent multiple instances of the app from running simultaneously
        private static readonly Mutex _mutex = new Mutex(true, "SmartSchedule-Unique-System-Mutex-Key-12345");

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!_mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("The application is already running!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Current.Shutdown();
                return;
            }


            base.OnStartup(e);

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }
    }

}
