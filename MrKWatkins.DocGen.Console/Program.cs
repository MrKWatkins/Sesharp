using MrKWatkins.DocGen.Markdown.Generation;
using MrKWatkins.DocGen.Model;
using MrKWatkins.DocGen.XmlDocumentation;
using Assembly = System.Reflection.Assembly;

var assemblyPath = args[0];
var xmlPath = assemblyPath.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);

var documentation = Documentation.Load(xmlPath);

var assembly = Assembly.LoadFile(assemblyPath);

var model = AssemblyParser.Parse(assembly, documentation);

AssemblyMarkdownGenerator.Generate(model, "/home/mrkwatkins/DocGenOutput");