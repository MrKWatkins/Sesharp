using MrKWatkins.Sesharp;
using MrKWatkins.Sesharp.Tool;
using Spectre.Console.Cli;

var app = new CommandApp();
app.RegisterSesharp(new RealFileSystem());
return app.Run(args);