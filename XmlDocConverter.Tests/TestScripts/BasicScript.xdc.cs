using System.Text;
using XmlDocConverter;

class Script
{
	static void Run()
	{
		Emit.Using(MarkdownEmitWriter.GitHub)
			.InDirectory(@"C:\Work\temp\xdc_test")
			.From(@"C:\Work\ProcessArgumentTools\ProcessArgumentTools\bin\Release\ProcessArgumentTools.dll")
			.Get.Assemblies.ForEach(emit =>
			{
				emit.ToFile("ProcessArgumentTools.md")
					.Write();

			});
	}
}