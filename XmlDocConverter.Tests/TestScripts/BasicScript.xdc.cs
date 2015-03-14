// require C:\Work\ProcessArgumentTools\ProcessArgumentTools\bin\Release\ProcessArgumentTools.dll

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

			.Write.A(
@"# Process Argument Tools

This .NET library provides some simple tools for creating and manipulating command line arguments.  It's main purpose is to:

1. Correctly quote and escape command line argument strings so they may be used by the OS.
2. Parse command lines into their separate arguments.
3. Provide strong types to represent escaped command line arguments.

It is *not* an options parsing library like [NDesk.Options](http://www.ndesk.org/Options) or [getopt](https://www.gnu.org/software/libc/manual/html_node/Getopt.html).
The Process Argument Tools does not interpret the contents of the argument strings other than to quote and escape the characters.


## Why?

The rules for properly quoting and escaping command line arguments tend to be more complicated than just putting double 
quotes around a string and replacing some characters.  For example, in Windows:

    'abc\\'    -> 'abc\\' (no quotes necessary)
    'a bc\\'   -> '""a bc\\\\""'
    'a bc\\ '  -> '""a bc\\ ""'
    'a bc\\"" ' -> '""a bc\\\\\"" ""'


## How to Use

This library is primary used via the Argument struct.  The Argument struct is a single argument string containing zero or 
more arguments in a format suitable for use by the OS.

	Argument argument = new Argument(  // FINISH


### Argument Policies

The rules and functionality for escaping and unescaping argument strings are defined by policy classes.  If no policy is
specified when creating an Argument the default policy for the current system will be used.

Arguments can be freely converted between policies.  If the target policy is not directly compatible with the source policy
the arguments will automatically be converted.

A set of standard argument policies are provided with this library:
")			
			// Write the list of argument policy classes.
			.Select.Classes(classes => classes.ForEach(emit => 
				{
					if (emit.Item.Type.IsSubclassOf(typeof(ProcessArgumentTools.Policy.ArgumentPolicy)))
						emit.Write.A("- ").Write.Link(emit.Item.FullName, emit.Item.Name).Write.L();
				}))
			.Write.A(
@"

Custom policies can be implemented by deriving from the ArugmentPolicy class.


## API Reference
")

			.Using(new EmitWriter<AssemblyContext>(item => item.Emit
				.Select.Structs(structs => structs.Render())
				.Select.Classes(classes => classes.Render())
			))

			.Using(new EmitWriter<ClassContext>(item => item.Emit
				.SetLinkTarget(item.Document.FullName, "#" + item.Document.FullName)
				.Write
					.L(@"### <a name=""{1}""></a>{0}", item.Document.Name, item.Document.FullName)
					.L()
				.Select.Doc(doc => doc.Render())
				.Write
					.L()
					.L()
				.Select.Properties(properties => properties
					.IfAny(emit => emit
						.Write
							.L("#### Properties")
						.Write.L()
						.Write
							.L("Property | Summary")
							.L("-------- | -------")
						.Render()
						.Write.L()))
				.Select.Methods(methods => methods
					.IfAny(emit => emit
						.Write
							.L("#### Methods")
							.L()
							.L("Method | Summary")
							.L("------ | -------")
						.Render()
						.Write.L()))
				.Select.Fields(fields => fields
					.IfAny(emit => emit
						.Write
							.L("#### Fields")
							.L()
							.L("Field | Summary")
							.L("----- | -------")
						.Render()
						.Write.L()))
			))

			.Using(new EmitWriter<StructContext>(item => item.Emit
				.SetLinkTarget(item.Document.FullName, "#" + item.Document.FullName)
				.Write.L("### {0}", item.Document.Name)
					.L()
				.Select.Doc(doc => doc.Render())
				.Write
					.L()
					.L()
				.Select.Properties(properties => properties
					.IfAny(emit => emit
						.Write
							.L("#### Properties")
							.L()
							.L("Property | Summary")
							.L("-------- | -------")
						.Render()
						.Write.L()))
				.Select.Methods(methods => methods
					.IfAny(emit => emit
						.Write
							.L("#### Methods")
							.L()
							.L("Method | Summary")
							.L("------ | -------")
						.Render()
						.Write.L()))
				.Select.Fields(fields => fields
					.IfAny(emit => emit
						.Write
							.L("#### Fields")
							.L()
							.L("Field | Summary")
							.L("----- | -------")
						.Render()
						.Write.L()))
			))

			.Using(new EmitWriter<FieldContext>(item => item.Emit
				.Write.A("{0} | ", item.Document.Name)
				.WithFilter(RenderFilter.CollapseNewlines.Then(RenderFilter.RegexReplace("\n", "<br>")),
					e2 => e2.Select.Doc(doc => doc.Select.Element(element => element.Render(), "summary")))
				.Write.L()
			))

			.Using(new EmitWriter<MethodContext>(item => item.Emit
				.Write.A("{0} | ", item.Document.Name)
				.WithFilter(RenderFilter.CollapseNewlines.Then(RenderFilter.RegexReplace("\n", "<br>")),
					e2 => e2.Select.Doc(doc => doc.Select.Element(element => element.Render(), "summary")))
				.Write.L()
			))

			.Using(new EmitWriter<PropertyContext>(item => item.Emit
				.Write.A("{0} | ", item.Document.Name)
				.WithFilter(RenderFilter.CollapseNewlines.Then(RenderFilter.RegexReplace("\n", "<br>")),
					e2 => e2.Select.Doc(doc => doc.Select.Element(element => element.Render(), "summary")))
				.Write.L()
			))

			.Render();
	}
}