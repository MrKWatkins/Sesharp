using MrKWatkins.Sesharp.Console;
using Spectre.Console.Cli;

var app = new CommandApp<DocGenCommand>();
return app.Run(args);