using MrKWatkins.DocGen.Markdown.Generation;
using MrKWatkins.DocGen.Model;
using MrKWatkins.DocGen.XmlDocumentation;
using Assembly = System.Reflection.Assembly;

var assemblyPath = args[0];
var xmlPath = assemblyPath.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);

var documentation = Documentation.Load(xmlPath);

var assembly = Assembly.LoadFile(assemblyPath);

var model = AssemblyParser.Parse(assembly, documentation);

var ast = model.Descendents.First(n => n.Name == "MrKWatkins.Ast");

var node = model.Descendents.First(n => n.Name == "Node`1");

AssemblyMarkdownGenerator.Generate(model, "/home/mrkwatkins/DocGenOutput");

return;