using Spectre.Console.Cli;

namespace MrKWatkins.Sesharp.Tool;

internal static class CommandAppExtensions
{
    internal static void RegisterSesharp(this CommandApp commandApp, IFileSystem fileSystem)
    {
        commandApp.SetDefaultCommand<DocGenCommand>();
        commandApp.Configure(config =>
        {
            config.Settings.ApplicationVersion = typeof(Program).Assembly.GetName().Version!.ToString();
            config.Settings.Registrar.RegisterInstance(fileSystem);
        });
    }
}