using System.Text;
using System.Linq;
using XmlDocConverter;
using XmlDocConverter.Fluent;

class Script
{
	static void Run()
	{
		var x =
		Emit.Begin
			.Using(MarkdownEmitWriter.GitHub)
			.InDirectory(@"C:\Work\temp\xdc_test")
			.From(@"C:\Work\ProcessArgumentTools\ProcessArgumentTools\bin\Release\ProcessArgumentTools.dll")
			.Select.Assemblies()
			//.Select2.Assemblies()
			//.Select.Assemblies()
			//.Select.Assemblies()
			;
			//.Get.Assemblies.ForEach(emit =>
			//{
			//	emit.ToFile("ProcessArgumentTools.md")
			//		.Write();

			//});
	}
}