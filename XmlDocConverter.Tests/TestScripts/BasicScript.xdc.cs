using System.Text;
using XmlDocConverter;

class Script
{
	static void Run()
	{
		Emit.From(@"C:\Work\ProcessArgumentTools\ProcessArgumentTools\bin\Release\ProcessArgumentTools.dll")
			.Using(MarkdownEmitWriter.GitHub)
			.InDirectory(@"C:\Work\temp\xdc_test")
			;
		//.Get.Assemblies.ForEach((emit, assembly) =>
		//	{
		//		emit.ToFile("ProcessArgumentTools.md")
		//			.Write(assembly);

		//	});
	}
}