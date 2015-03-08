using System.Text;
using System.Linq;
using XmlDocConverter;
using XmlDocConverter.Fluent;

class Script
{
	static void Run()
	{
		var y =
		Emit.BeginClean
			.Break()
			.Break()
			.Break()
			;

		var x =
		Emit.Begin
			.Using(MarkdownEmitWriter.GitHub)
			.InDirectory(@"C:\Work\temp\xdc_test")
			.From(@"C:\Work\ProcessArgumentTools\ProcessArgumentTools\bin\Release\ProcessArgumentTools.dll")
			.ToFile("ProcessArgumentTools.md")
			.Write()
			.Using(new EmitWriter<RootContext>(context => context))
			.Using((EmitContext<RootContext> context) =>
				{
					return context;
				})
			.WriteHeader("asdf")
			.Write()
			//.Select.Assemblies().UsingX(null)
			.Break()
			.Write()
			.Select.Assemblies().ForEach(emit =>
			{
				emit.Select.Classes()
					.ForEach(emi2 =>
					{
					});
			})
			.Select.Assemblies()
			.Break()
			//.Select.Assemblies().ForEach(emit =>
			//{
			//	emit.Select.Classes();
			//})
			//.Select.Assemblies().ForEach(emit =>
			//{
			//})
			//.Select.Classes()
			;
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