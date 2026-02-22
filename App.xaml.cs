using System;
using System.Windows;
using System.Windows.Threading;

namespace laba1
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ловим критические ошибки до запуска UI
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                Exception ex = args.ExceptionObject as Exception;
                MessageBox.Show($"КРИТИЧЕСКАЯ ОШИБКА:\n{ex?.Message}\n\n{ex?.StackTrace}",
                                "Ошибка запуска", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Ловим ошибки в потоке интерфейса
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"ОШИБКА:\n{args.Exception.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}