using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeGenerators.CodeFirst;

namespace EntityGenerator.Engine
{

	/// <summary>
	/// Class to produce the template output
	/// </summary>

	[global::System.CodeDom.Compiler.GeneratedCodeAttribute( "Microsoft.VisualStudio.TextTemplating", "14.0.0.0" )]
	public partial class MappingClassTemplate : MappingClassTemplateBase
	{
		#region MappingClassTemplate Members

		/// <summary>
		/// Create the template output
		/// </summary>
		public virtual string TransformText()
		{
			this.Write( "using System.ComponentModel.DataAnnotations.Schema;\r\nusing System.Data.Entity.Mod" +
					"elConfiguration;\r\n\r\nnamespace " );

			this.Write( this.ToStringHelper.ToStringWithCulture( mNamespace ) );

#line default
#line hidden
			this.Write( "\r\n{\r\n\tpublic class " );

			this.Write( this.ToStringHelper.ToStringWithCulture( mTableAlias ) );

#line default
#line hidden
			this.Write( "Map : EntityTypeConfiguration<" );

			this.Write( this.ToStringHelper.ToStringWithCulture( mTableAlias ) );

#line default
#line hidden
			this.Write( ">\r\n\t{\r\n\t\t#region Constructors\r\n\r\n\t\tpublic " );


			this.Write( this.ToStringHelper.ToStringWithCulture( mTableAlias ) );

#line default
#line hidden
			this.Write( "Map()\r\n\t\t{\r\n\t\t\t// Primary Key\r\n" );



			var primaryKeyColumns = mGenerator.GetPrimaryKeyColumns( mTableName );
			if( primaryKeyColumns.Count() == 1 )
			{


#line default
#line hidden
				this.Write( "\t\t\tHasKey( model => model." );

				this.Write( this.ToStringHelper.ToStringWithCulture( primaryKeyColumns.Single() ) );

#line default
#line hidden
				this.Write( " );\r\n" );



			}
			else if( primaryKeyColumns.Count() == 0 )
			{


#line default
#line hidden
				this.Write( "\t\t\t//TODO: Generated without primary keys. Please select primary keys.\r\n\t\t\t//TODO" +
						": Also will need to put [key] attribute on any model properties used here.\r\n\t\t\tH" +
						"asKey( model => model. );\r\n" );


			}
			else
			{


#line default
#line hidden
				this.Write( "\t\t\tHasKey( model => new { " );


				this.Write( this.ToStringHelper.ToStringWithCulture( string.Join( ", ", primaryKeyColumns.Select( key => "model." + key ) ) ) );

#line default
#line hidden
				this.Write( " });\r\n" );



			}


#line default
#line hidden
			this.Write( "\r\n\t\t\t// Properties\r\n" );



			var columnMappingInfos = mGenerator.GetColumnMappingInfos( mTableName );

			foreach( var columnMap in columnMappingInfos )
			{
				var configLines = new List<string>();

				if( columnMap.IsPrimaryKey )
				{
					if( columnMap.IsStorageGenerated )
					{
						configLines.Add( ".HasDatabaseGeneratedOption( DatabaseGeneratedOption.Identity )" );
					}
					else
					{
						configLines.Add( ".HasDatabaseGeneratedOption( DatabaseGeneratedOption.None )" );
					}
				}

				if( columnMap.Type.Contains( "decimal" ) &&
					columnMap.Precision != null &&
					columnMap.Scale != null )
				{
					configLines.Add( string.Format( ".HasPrecision( {0}, {1} )", columnMap.Precision.Value, columnMap.Scale.Value ) );
				}

				if( columnMap.Type == "string" || columnMap.Type == "byte[]" )
				{
					if( !columnMap.IsNullable )
					{
						configLines.Add( ".IsRequired()" );
					}

					if( columnMap.IsUnicode )
					{
						configLines.Add( ".IsUnicode()" );
					}
					else if( columnMap.Type == "string" )
					{
						configLines.Add( ".IsUnicode( false )" );
					}

					if( columnMap.IsFixedLength )
					{
						configLines.Add( ".IsFixedLength()" );
					}

					if( columnMap.MaxLength != null )
					{
						if( columnMap.MaxLength.Value <= 0 )
						{
							configLines.Add( ".IsMaxLength()" );
						}
						else
						{
							configLines.Add( string.Format( ".HasMaxLength( {0} )", columnMap.MaxLength.Value ) );
						}
					}
					if( columnMap.IsRowVersion )
					{
						configLines.Add( ".IsFixedLength()" );
						configLines.Add( ".HasMaxLength( 8 )" );
						configLines.Add( ".IsRowVersion()" );
					}
				}

				if( configLines.Any() )
				{


#line default
#line hidden
					this.Write( "\t\t\tProperty( model => model." );


					this.Write( this.ToStringHelper.ToStringWithCulture( columnMap.ColumnName ) );

#line default
#line hidden
					this.Write( " )\r\n\t\t\t\t" );

					this.Write( this.ToStringHelper.ToStringWithCulture( string.Join( "\r\n				", configLines ) ) );

#line default
#line hidden
					this.Write( ";\r\n\r\n" );



				}
			}


#line default
#line hidden
			this.Write( "\t\t\t// Table & Column Mappings\r\n\t\t\tToTable( \"" );


			this.Write( this.ToStringHelper.ToStringWithCulture( mTableName ) );

#line default
#line hidden
			this.Write( "\" );\r\n\r\n" );



			foreach( var columnMap in columnMappingInfos )
			{


#line default
#line hidden
				this.Write( "\t\t\tProperty( model => model." );

				this.Write( this.ToStringHelper.ToStringWithCulture( columnMap.ColumnName ) );

#line default
#line hidden
				this.Write( " ).HasColumnName( \"" );

				this.Write( this.ToStringHelper.ToStringWithCulture( columnMap.ColumnName ) );

#line default
#line hidden
				this.Write( "\" );\r\n" );


			}

			// Find m:m relationshipsto configure 
			var manyManyRelationships = mGenerator.GetManyManyRelationships( mTableName );

			// Find FK relationships that this entity is the dependent of
			var fkRelationships = mGenerator.GetForeignKeyRelationships( mTableName );

			if( manyManyRelationships.Any() || fkRelationships.Any() )
			{


#line default
#line hidden
				this.Write( "\r\n\t\t\t// Relationships\r\n" );



				// Many To Many
				foreach( var navProperty in manyManyRelationships )
				{
					var leftColumns = string.Join( ", ", navProperty.LeftForeignKeyColumnNames.Select( col => "\"" + col + "\"" ) );
					var rightColumns = string.Join( ", ", navProperty.RightForeignKeyColumnNames.Select( col => "\"" + col + "\"" ) );


#line default
#line hidden
					this.Write( "\t\t\tHasMany( model => model." );

					this.Write( this.ToStringHelper.ToStringWithCulture( navProperty.RightPluralizedTableName ) );

#line default
#line hidden
					this.Write( " )\r\n\t\t\t\t.WithMany( reverse => reverse." );


					this.Write( this.ToStringHelper.ToStringWithCulture( navProperty.LeftPluralizedTableName ) );

#line default
#line hidden
					this.Write( " )\r\n\t\t\t\t.Map( map =>\r\n\t\t\t\t{\r\n\t\t\t\t\tmap.ToTable( \"" );


					this.Write( this.ToStringHelper.ToStringWithCulture( navProperty.ManyToManyTableName ) );

#line default
#line hidden
					this.Write( "\" );\r\n\t\t\t\t\tmap.MapLeftKey( " );


					this.Write( this.ToStringHelper.ToStringWithCulture( leftColumns ) );

#line default
#line hidden
					this.Write( " );\r\n\t\t\t\t\tmap.MapRightKey( " );


					this.Write( this.ToStringHelper.ToStringWithCulture( rightColumns ) );

#line default
#line hidden
					this.Write( " );\r\n\t\t\t\t} );\r\n\r\n" );



				}

				// One To Many
				// One To One
				foreach( var navProperty in fkRelationships )
				{
					switch( navProperty.RelationshipType )
					{
						case RelationshipType.OneToOneDependent:


#line default
#line hidden
							this.Write( "\t\t\tHasRequired( model => model." );


							this.Write( this.ToStringHelper.ToStringWithCulture( navProperty.ReferenceTableName ) );

#line default
#line hidden
							this.Write( " )\r\n\t\t\t\t.WithOptional( reverse => reverse." );

							this.Write( this.ToStringHelper.ToStringWithCulture( navProperty.SourceTableName ) );

#line default
#line hidden
							this.Write( " );\r\n" );


							break;
						case RelationshipType.OneToMany:
							if( navProperty.IsForeignKeyOptional )
							{


#line default
#line hidden
								this.Write( "\t\t\tHasOptional( model => model." );


								this.Write( this.ToStringHelper.ToStringWithCulture( navProperty.ReferenceTableName ) );

#line default
#line hidden
								this.Write( " )\r\n" );


							}
							else
							{


#line default
#line hidden
								this.Write( "\t\t\tHasRequired( model => model." );


								this.Write( this.ToStringHelper.ToStringWithCulture( navProperty.ReferenceTableName ) );

#line default
#line hidden
								this.Write( " )\r\n" );



							}


#line default
#line hidden
							this.Write( "\t\t\t\t.WithMany( reverse => reverse." );

							this.Write( this.ToStringHelper.ToStringWithCulture( navProperty.SourcePluralizedTableName ) );

#line default
#line hidden
							this.Write( " )\r\n" );


							if( navProperty.SourceForeignKeyColumnNames.Count() == 1 )
							{


#line default
#line hidden
								this.Write( "\t\t\t\t.HasForeignKey( model => model." );


								this.Write( this.ToStringHelper.ToStringWithCulture( navProperty.SourceForeignKeyColumnNames.First() ) );

#line default
#line hidden
								this.Write( " );\r\n\r\n" );



							}
							else
							{


#line default
#line hidden
								this.Write( "\t\t\t\t.HasForeignKey( model => new { " );


								this.Write( this.ToStringHelper.ToStringWithCulture( string.Join( ", ", navProperty.SourceForeignKeyColumnNames.Select( fk => "model." + fk ) ) ) );

#line default
#line hidden
								this.Write( " } );\r\n\r\n" );



							}
							break;
					}
				}
			}


#line default
#line hidden
			this.Write( "\t\t}\r\n\r\n\t\t#endregion Constructors\r\n\r\n\t}\r\n}\r\n" );
			return this.GenerationEnvironment.ToString();
		}

		#endregion MappingClassTemplate Members

	}

#line default
#line hidden
	#region Base class
	/// <summary>
	/// Base class for this transformation
	/// </summary>
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute( "Microsoft.VisualStudio.TextTemplating", "14.0.0.0" )]
	public class MappingClassTemplateBase
	{
		#region MappingClassTemplateBase Members

		/// <summary>
		/// Gets the current indent we use when adding lines to the output
		/// </summary>
		public string CurrentIndent
		{
			get
			{
				return this.currentIndentField;
			}
		}

		/// <summary>
		/// The error collection for the generation process
		/// </summary>
		public System.CodeDom.Compiler.CompilerErrorCollection Errors
		{
			get
			{
				if( (this.errorsField == null) )
				{
					this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
				}
				return this.errorsField;
			}
		}

		/// <summary>
		/// Current transformation session
		/// </summary>
		public virtual global::System.Collections.Generic.IDictionary<string, object> Session
		{
			get
			{
				return this.sessionField;
			}
			set
			{
				this.sessionField = value;
			}
		}

		/// <summary>
		/// Helper to produce culture-oriented representation of an object as a string
		/// </summary>
		public ToStringInstanceHelper ToStringHelper
		{
			get
			{
				return this.toStringHelperField;
			}
		}

		/// <summary>
		/// Remove any indentation
		/// </summary>
		public void ClearIndent()
		{
			this.indentLengths.Clear();
			this.currentIndentField = "";
		}

		/// <summary>
		/// Raise an error
		/// </summary>
		public void Error( string message )
		{
			System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
			error.ErrorText = message;
			this.Errors.Add( error );
		}

		/// <summary>
		/// Remove the last indent that was added with PushIndent
		/// </summary>
		public string PopIndent()
		{
			string returnValue = "";
			if( (this.indentLengths.Count > 0) )
			{
				int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
				this.indentLengths.RemoveAt( (this.indentLengths.Count - 1) );
				if( (indentLength > 0) )
				{
					returnValue = this.currentIndentField.Substring( (this.currentIndentField.Length - indentLength) );
					this.currentIndentField = this.currentIndentField.Remove( (this.currentIndentField.Length - indentLength) );
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Increase the indent
		/// </summary>
		public void PushIndent( string indent )
		{
			if( (indent == null) )
			{
				throw new global::System.ArgumentNullException( "indent" );
			}
			this.currentIndentField = (this.currentIndentField + indent);
			this.indentLengths.Add( indent.Length );
		}

		/// <summary>
		/// Raise a warning
		/// </summary>
		public void Warning( string message )
		{
			System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
			error.ErrorText = message;
			error.IsWarning = true;
			this.Errors.Add( error );
		}

		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		public void Write( string textToAppend )
		{
			if( string.IsNullOrEmpty( textToAppend ) )
			{
				return;
			}
			// If we're starting off, or if the previous text ended with a newline,
			// we have to append the current indent first.
			if( ((this.GenerationEnvironment.Length == 0)
						|| this.endsWithNewline) )
			{
				this.GenerationEnvironment.Append( this.currentIndentField );
				this.endsWithNewline = false;
			}
			// Check if the current text ends with a newline
			if( textToAppend.EndsWith( global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture ) )
			{
				this.endsWithNewline = true;
			}
			// This is an optimization. If the current indent is "", then we don't have to do any
			// of the more complex stuff further down.
			if( (this.currentIndentField.Length == 0) )
			{
				this.GenerationEnvironment.Append( textToAppend );
				return;
			}
			// Everywhere there is a newline in the text, add an indent after it
			textToAppend = textToAppend.Replace( global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField) );
			// If the text ends with a newline, then we should strip off the indent added at the very end
			// because the appropriate indent will be added when the next time Write() is called
			if( this.endsWithNewline )
			{
				this.GenerationEnvironment.Append( textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length) );
			}
			else
			{
				this.GenerationEnvironment.Append( textToAppend );
			}
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		public void Write( string format, params object[] args )
		{
			this.Write( string.Format( global::System.Globalization.CultureInfo.CurrentCulture, format, args ) );
		}

		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		public void WriteLine( string textToAppend )
		{
			this.Write( textToAppend );
			this.GenerationEnvironment.AppendLine();
			this.endsWithNewline = true;
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		public void WriteLine( string format, params object[] args )
		{
			this.WriteLine( string.Format( global::System.Globalization.CultureInfo.CurrentCulture, format, args ) );
		}

		#endregion MappingClassTemplateBase Members

		#region Fields

		private bool endsWithNewline;
		private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
		private global::System.Collections.Generic.IDictionary<string, object> sessionField;
		private global::System.Collections.Generic.List<int> indentLengthsField;
		private string currentIndentField = "";
		private global::System.Text.StringBuilder generationEnvironmentField;
		private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();

		#endregion Fields

		#region Private Members

		/// <summary>
		/// A list of the lengths of each indent that was added with PushIndent
		/// </summary>
		private System.Collections.Generic.List<int> indentLengths
		{
			get
			{
				if( (this.indentLengthsField == null) )
				{
					this.indentLengthsField = new global::System.Collections.Generic.List<int>();
				}
				return this.indentLengthsField;
			}
		}

		#endregion Private Members

		#region Protected Members

		/// <summary>
		/// The string builder that generation-time code is using to assemble generated output
		/// </summary>
		protected System.Text.StringBuilder GenerationEnvironment
		{
			get
			{
				if( (this.generationEnvironmentField == null) )
				{
					this.generationEnvironmentField = new global::System.Text.StringBuilder();
				}
				return this.generationEnvironmentField;
			}
			set
			{
				this.generationEnvironmentField = value;
			}
		}

		#endregion Protected Members

		#region Other

		/// <summary>
		/// Utility class to produce culture-oriented representation of an object as a string.
		/// </summary>
		public class ToStringInstanceHelper
		{
			#region ToStringInstanceHelper Members

			/// <summary>
			/// Gets or sets format provider to be used by ToStringWithCulture method.
			/// </summary>
			public System.IFormatProvider FormatProvider
			{
				get
				{
					return this.formatProviderField;
				}
				set
				{
					if( (value != null) )
					{
						this.formatProviderField = value;
					}
				}
			}

			/// <summary>
			/// This is called from the compile/run appdomain to convert objects within an expression block to a string
			/// </summary>
			public string ToStringWithCulture( object objectToConvert )
			{
				if( (objectToConvert == null) )
				{
					throw new global::System.ArgumentNullException( "objectToConvert" );
				}
				System.Type t = objectToConvert.GetType();
				System.Reflection.MethodInfo method = t.GetMethod( "ToString", new System.Type[] {
							typeof(System.IFormatProvider)} );
				if( (method == null) )
				{
					return objectToConvert.ToString();
				}
				else
				{
					return ((string)(method.Invoke( objectToConvert, new object[] {
								this.formatProviderField } )));
				}
			}

			#endregion ToStringInstanceHelper Members

			#region Fields

			private System.IFormatProvider formatProviderField = global::System.Globalization.CultureInfo.InvariantCulture;

			#endregion Fields

		}

		#endregion Other

	}
	#endregion
}
