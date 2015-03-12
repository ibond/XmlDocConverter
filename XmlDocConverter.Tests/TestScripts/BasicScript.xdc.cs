using System.Text;
using System.Linq;
using XmlDocConverter;
using XmlDocConverter.Fluent;
using RazorEngine;

class Script
{
	static void Run()
	{
		Emit.Begin
			.Using(MarkdownEmitWriter.GitHub)
			.InDirectory(@"C:\Work\temp\xdc_test")
			.From(@"C:\Work\ProcessArgumentTools\ProcessArgumentTools\bin\Release\ProcessArgumentTools.dll")
			.ToFile("ProcessArgumentTools.md")

			.Using(new EmitWriter<AssemblyContext>(item => item.Emit
				.Write.L("# {0}", item.Document.Name)
				.Write.L()
				.Select.Structs().Render()
				.Select.Classes().Render()
			))

			.Using(new EmitWriter<ClassContext>(item => item.Emit
				.Write.L("## {0}", item.Document.Name)
				.Write.L()
				.Select.Doc().Render()
				.Write.L()
				.Write.L()
				.Select.Properties().IfAny(emit => emit
					.Write.L("### Properties")
					.Write.L()
					.Write.L("Property | Summary")
					.Write.L("-------- | -------")
					.Render()
					.Write.L())
					.Break()
				.Select.Methods().IfAny(emit => emit
					.Write.L("### Methods")
					.Write.L()
					.Write.L("Method | Summary")
					.Write.L("------ | -------")
					.Render()
					.Write.L())
					.Break()
				.Select.Fields().IfAny(emit => emit
					.Write.L("### Fields")
					.Write.L()
					.Write.L("Field | Summary")
					.Write.L("----- | -------")
					.Render()
					.Write.L())
					.Break()
			))

			.Using(new EmitWriter<StructContext>(item => item.Emit
				.Write.L("## {0}", item.Document.Name)
				.Write.L()
				.Select.Doc().Render()
				.Write.L()
				.Write.L()
				.Select.Properties().IfAny(emit => emit
					.Write.L("### Properties")
					.Write.L()
					.Write.L("Property | Summary")
					.Write.L("-------- | -------")
					.Render()
					.Write.L())
					.Break()
				.Select.Methods().IfAny(emit => emit
					.Write.L("### Methods")
					.Write.L()
					.Write.L("Method | Summary")
					.Write.L("------ | -------")
					.Render()
					.Write.L())
					.Break()
				.Select.Fields().IfAny(emit => emit
					.Write.L("### Fields")
					.Write.L()
					.Write.L("Field | Summary")
					.Write.L("----- | -------")
					.Render()
					.Write.L())
					.Break()
			))

			.Using(new EmitWriter<FieldContext>(item => item.Emit
				.Write.A("{0} | ", item.Document.Name)
				.WithFilter(RenderFilter.Chain(RenderFilter.CollapseNewlines, RenderFilter.RegexReplace("\r\n|\r|\n", "<br>")),
					e2 => e2.Select.Doc().Select.Element("summary").Render())
				.Write.L()
			))

			.Using(new EmitWriter<MethodContext>(item => item.Emit
				.Write.A("{0} | ", item.Document.Name)
				.WithFilter(RenderFilter.Chain(RenderFilter.CollapseNewlines, RenderFilter.RegexReplace("\r\n|\r|\n", "<br>")),
					e2 => e2.Select.Doc().Select.Element("summary").Render())
				.Write.L()
			))

			.Using(new EmitWriter<PropertyContext>(item => item.Emit
				.Write.A("{0} | ", item.Document.Name)
				.WithFilter(RenderFilter.Chain(RenderFilter.CollapseNewlines, RenderFilter.RegexReplace("\r\n|\r|\n", "<br>")),
					e2 => e2.Select.Doc().Select.Element("summary").Render())
				.Write.L()
			))

			.Render();
				

			;
	}
}