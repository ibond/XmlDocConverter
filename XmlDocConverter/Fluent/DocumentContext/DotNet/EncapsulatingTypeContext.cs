using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This is a common base class for ClassContext and StructContext since they act mostly the same for documentation
	/// purposes.
	/// </summary>
	/// <typeparam name="TDerived">The derived context type.</typeparam>
	public class EncapsulatingTypeContext<TDerived> : TypeContext<TDerived>, 
		FieldContext.IProvider, 
		PropertyContext.IProvider,
		MethodContext.IProvider
		where TDerived : EncapsulatingTypeContext<TDerived>
	{
		/// <summary>
		/// Construct an EncapsulatingTypeContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		/// <param name="classType">The Type for the class or struct contained within this context.</param>
		public EncapsulatingTypeContext(DocumentSource documentSource, Type type)
			: base(documentSource, type)
		{
			Contract.Requires(type.IsClass || (type.IsValueType && !type.IsEnum && !type.IsPrimitive));
		}

		/// <summary>
		/// Get all of the fields contained within this class.
		/// </summary>
		/// <returns>The fields in this class.</returns>
		public new IEnumerable<FieldContext> GetFields(BindingFlags bindingFlags) { return base.GetFields(bindingFlags); }

		/// <summary>
		/// Get all of the properties contained within this class.
		/// </summary>
		/// <returns>The properties in this class.</returns>
		public new IEnumerable<PropertyContext> GetProperties(BindingFlags bindingFlags) { return base.GetProperties(bindingFlags); }

		/// <summary>
		/// Get all of the methods contained within this class.
		/// </summary>
		/// <returns>The methods in this class.</returns>
		public new IEnumerable<MethodContext> GetMethods(BindingFlags bindingFlags) { return base.GetMethods(bindingFlags); }

		/// <summary>
		/// The default writer for a class.
		/// </summary>
		protected override EmitWriter<TDerived>.Writer GetDefaultRenderer()
		{
			return item => item.Emit
				.Select.Properties(properties => properties.Render())
				.Select.Methods(methods => methods.Render())
				.Select.Fields(fields => fields.Render());
		}
	}
}
