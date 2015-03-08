using System.Text;
using System.Linq;
using XmlDocConverter;
using XmlDocConverter.Fluent;

class Script
{
	static void Run()
	{
		Emit.Begin
			.Using(MarkdownEmitWriter.GitHub)
			.InDirectory(@"C:\Work\temp\xdc_test")
			.From(@"C:\Work\ProcessArgumentTools\ProcessArgumentTools\bin\Release\ProcessArgumentTools.dll")
			.ToFile("ProcessArgumentTools.md")
			.Write()
			;
	}
}