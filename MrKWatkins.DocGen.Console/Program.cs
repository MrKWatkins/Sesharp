using MrKWatkins.DocGen.Markdown;
using MrKWatkins.DocGen.Model;
using MrKWatkins.DocGen.XmlDocumentation;
using Assembly = System.Reflection.Assembly;

var assemblyPath = args[0];

var assembly = Assembly.LoadFile(assemblyPath);

var model = AssemblyParser.Parse(assembly);

var ast = model.Descendents.First(n => n.Name == "MrKWatkins.Ast");

var node = model.Descendents.First(n => n.Name == "Node`1");

var xmlPath = assemblyPath.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);

var documentation = Documentation.Load(xmlPath);

MarkdownGenerator.Generate(model, "/home/mrkwatkins/DocGenOutput");

return;