                                              using System;
using System.Windows;

namespace ChessTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            #if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            Application.Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
            #endif
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = new Game();
            mainWindow.Show();
        }
    }
}                                                                                                                                                                                                                                                                         