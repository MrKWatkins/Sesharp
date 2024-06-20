using MrKWatkins.DocGen.Console;
using Spectre.Console.Cli;

var app = new CommandApp<DocGenCommand>();
return app.Run(args);