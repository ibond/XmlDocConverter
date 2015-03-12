using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;
using XmlDocConverter.Fluent.Detail;
using System.Runtime.CompilerServices;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// A base context for .NET types.
	/// </summary>
	public abstract class TypeContext<TDerived> : DotNetDocumentContext<TDerived>, DocEntryContext.IProvider
		where TDerived : TypeContext<TDerived>
	{
		/// <summary>
		/// Construct an TypeContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		public TypeContext(DocumentSource documentSource, Type type)
			: base(documentSource)
		{
			Contract.Requires(type != null);
			Contract.Ensures(m_type != null);

			m_type = type;
		}
		
		/// <summary>
		/// Get all of the fields contained within this class.
		/// </summary>
		/// <returns>The fields in this class.</returns>
		protected IEnumerable<FieldContext> GetFields(BindingFlags bindingFlags)
		{
			return m_type.GetFields(bindingFlags)
				.Where(field => Attribute.GetCustomAttribute(field, typeof(CompilerGeneratedAttribute)) == null)
				.Select(field => new FieldContext(DocumentSource, field));
		}

		/// <summary>
		/// Get all of the properties contained within this class.
		/// </summary>
		/// <returns>The properties in this class.</returns>
		protected IEnumerable<PropertyContext> GetProperties(BindingFlags bindingFlags)
		{
			return m_type.GetProperties(bindingFlags)
				.Where(property => Attribute.GetCustomAttribute(property, typeof(CompilerGeneratedAttribute)) == null)
				.Select(property => new PropertyContext(DocumentSource, property));
		}

		/// <summary>
		/// Get all of the methods contained within this class.
		/// </summary>
		/// <returns>The methods in this class.</returns>
		protected IEnumerable<MethodContext> GetMethods(BindingFlags bindingFlags)
		{
			// Ignore property getter and setter methods.
			return m_type.GetMethods(bindingFlags)
				.Where(method => Attribute.GetCustomAttribute(method, typeof(CompilerGeneratedAttribute)) == null)
				.Where(method => !method.DeclaringType.GetProperties().Any(property => property.GetMethod == method || property.SetMethod == method))
				.Select(method => new MethodContext(DocumentSource, method));
		}

		/// <summary>
		/// Get the documentation entry for this type.
		/// </summary>
		/// <returns>The doc entry for this type.</returns>
		public DocEntryContext GetDocEntry()
		{
			return new DocEntryContext(DocumentSource, DocumentSource.TryGetEntry(Type));
		}
		
		/// <summary>
		/// Gets the name of this type.
		/// </summary>
		public string Name { get { return m_type.Name; } }

		/// <summary>
		/// Gets the type info.
		/// </summary>
		public Type Type { get { return m_type; } }

		/// <summary>
		/// The info for this type.
		/// </summary>
		private readonly Type m_type;
	}
}
