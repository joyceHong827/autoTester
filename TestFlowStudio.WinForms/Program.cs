using Serilog;
using TestFlowStudio.Core.Helpers;

namespace TestFlowStudio.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/testflow_.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        ApplicationConfiguration.Initialize();
        Application.ThreadException += (_, e) =>
            Log.Error(e.Exception, "Unhandled UI thread exception");
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Log.Error(e.ExceptionObject as Exception, "Unhandled domain exception");

        try
        {
            Application.Run(new Forms.MainForm());
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
