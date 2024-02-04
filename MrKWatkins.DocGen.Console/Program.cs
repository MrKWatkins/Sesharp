using MrKWatkins.DocGen.Markdown.Generation;
using MrKWatkins.DocGen.Model;
using MrKWatkins.DocGen.Writerside;
using MrKWatkins.DocGen.XmlDocumentation;
using Assembly = System.Reflection.Assembly;

var assemblyPath = args[0];
var writersideOptions = new WritersideOptions(args[1]);

if (args.Length > 2)
{
    writersideOptions.TreeFile = args[2];
}

var xmlPath = assemblyPath.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);

var documentation = Documentation.Load(xmlPath);

var assembly = Assembly.LoadFile(assemblyPath);

var assemblyDetails = AssemblyParser.Parse(assembly, documentation);

if (writersideOptions.DeleteContentsOfOutputDirectory)
{
    Directory.Delete(writersideOptions.OutputDirectoryPath, true);
    Directory.CreateDirectory(writersideOptions.OutputDirectoryPath);
}

WritersideXmlGenerator.UpdateWriterside(writersideOptions, assemblyDetails);

AssemblyMarkdownGenerator.Generate(assemblyDetails, writersideOptions.OutputDirectoryPath);