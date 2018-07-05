using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;

namespace CodeGenerators.CodeFirst
{
	public class CodeFirstGenerator
	{
		#region Constructors

		private CodeFirstGenerator( string connectionString )
		{
			mCodeFirstDal = new CodeFirstDal( connectionString );
			mManyToManyMappingTablesCache = new HashSet<string>( mCodeFirstDal.GetManyToManyMappingTables() );
		}

		private CodeFirstGenerator( string connectionString, string[][] tablesAndAliases ) : this(connectionString)
		{
			mTablesAndAliasesCache = tablesAndAliases.ToDictionary( key => key[0], value => value[1] );
		}

		private CodeFirstGenerator( string connectionString, Dictionary<string,string> tablesAndAliases ) : this( connectionString )
		{
			mTablesAndAliasesCache = tablesAndAliases;
		}

		#endregion Constructors

		#region CodeFirstGenerator Members

		public IEnumerable<CollectionNavigationProperty> GetCollectionNavigationProperties( string tableName )
		{
			var mappings = mCodeFirstDal.GetTableMappingInfo( tableName );
			var navigationProperties = new List<CollectionNavigationProperty>();
			var pluralizationService = PluralizationService.CreateService( CultureInfo.CurrentCulture );

			var usedPropertyNames = new Dictionary<string, Dictionary<string, int>>();

			// Many To One
			foreach( var singleRelationship in mappings.Where( map =>
				_DetermineRelationshipType( tableName, map ) == RelationshipType.ManyToOne ) )
			{
				var otherCollectionSubType = _GetTableAlias( singleRelationship.SourceTable );

				var thisCollectionPropertyName = _GetPropertyName( usedPropertyNames, singleRelationship.ReferenceTable, pluralizationService.Pluralize( _GetTableAlias( singleRelationship.SourceTable ) ) );
				var otherCollectionPropertyName = _GetPropertyName( usedPropertyNames, singleRelationship.SourceTable, _GetTableAlias( singleRelationship.ReferenceTable ) );

				var navProperty = new CollectionNavigationProperty
				{
					PropertyName = thisCollectionPropertyName,
					PropertyType = otherCollectionSubType,
					RelationshipType = RelationshipType.ManyToOne,
					ReversePropertyName = otherCollectionPropertyName,
					IsNullable = mCodeFirstDal.GetForeignKeyColumns( singleRelationship.SourceTable, singleRelationship.FKName ).All( key => key.Item2 )
				};

				navigationProperties.Add( navProperty );
			}

			// Many To Many
			foreach( var singleRelationship in mappings.Where( map =>
				_DetermineRelationshipType( tableName, map ) == RelationshipType.ManyToMany ) )
			{
				var manyMappings = mCodeFirstDal.GetTableMappingInfo( singleRelationship.SourceTable );

				var thisMany = manyMappings.First( map => map.FKName == singleRelationship.FKName );
				var otherMany = manyMappings.First( map => map.FKName != singleRelationship.FKName );

				var collectionSubType = _GetTableAlias( otherMany.ReferenceTable );

				var thisCollectionPropertyName = _GetPropertyName( usedPropertyNames, thisMany.ReferenceTable, pluralizationService.Pluralize( _GetTableAlias( otherMany.ReferenceTable ) ) );
				var otherCollectionPropertyName = _GetPropertyName( usedPropertyNames, otherMany.ReferenceTable, pluralizationService.Pluralize( _GetTableAlias( thisMany.ReferenceTable ) ) );

				var navProperty = new CollectionNavigationProperty
				{
					PropertyName = thisCollectionPropertyName,
					PropertyType = collectionSubType,
					RelationshipType = RelationshipType.ManyToMany,
					ReversePropertyName = otherCollectionPropertyName
				};

				navigationProperties.Add( navProperty );
			}

			return navigationProperties;
		}

		public IEnumerable<ColumnMappingInfo> GetColumnMappingInfos( string tableName )
		{
			return mCodeFirstDal.GetColumnMappingInfos( tableName );
		}

		public IEnumerable<ForeignKeyRelationship> GetForeignKeyRelationships( string tableName )
		{
			var pluralizationService = PluralizationService.CreateService( CultureInfo.CurrentCulture );

			var usedPropertyNames = new Dictionary<string, Dictionary<string, int>>();

			var foreignKeyRelationships =
				mCodeFirstDal.GetTableMappingInfo( tableName )
				.Where( map => map.SourceTable == tableName )
				.Select( map => new ForeignKeyRelationship
				{
					SourceForeignKeyColumnNames = mCodeFirstDal.GetForeignKeyColumns( tableName, map.FKName ).Select( column => column.Item1 ),
					SourcePluralizedTableName = _GetPropertyName( usedPropertyNames, map.ReferenceTable, pluralizationService.Pluralize( _GetTableAlias( map.SourceTable ) ) ),
					SourceTableName = _GetTableAlias( map.SourceTable ),
					ReferenceTableName = _GetTableAlias( map.ReferenceTable ),
					IsForeignKeyOptional = mCodeFirstDal.GetForeignKeyColumns( tableName, map.FKName ).Any( column => column.Item2 ),
					RelationshipType = _DetermineRelationshipType( tableName, map )
				} )
				.ToList();

			return foreignKeyRelationships;
		}

		public IEnumerable<ManyManyRelationship> GetManyManyRelationships( string tableName )
		{
			var pluralizationService = PluralizationService.CreateService( CultureInfo.CurrentCulture );

			var manyManyMappings = mCodeFirstDal.GetTableMappingInfo( tableName )
				.Where( map => _DetermineRelationshipType( tableName, map ) == RelationshipType.ManyToMany );

			var usedPropertyNames = new Dictionary<string, Dictionary<string, int>>();

			var manyManyRelationships = new List<ManyManyRelationship>();
			foreach( var manyManyMapping in manyManyMappings )
			{
				var manyMappings = mCodeFirstDal.GetTableMappingInfo( manyManyMapping.SourceTable );

				// Select the first table alphabetically, it is important to do this consistently
				// since we only want to model one side of many to many relationship
				var leftMapping = manyMappings.OrderBy( map => map.ReferenceTable ).First();
				var rightMapping = manyMappings.OrderBy( map => map.ReferenceTable ).Last();

				// This enforces that only one of the many to many table gets the mapping info.
				if( leftMapping.ReferenceTable == tableName )
				{
					var manyManyRelationship = new ManyManyRelationship
					{
						ManyToManyTableName = rightMapping.SourceTable,

						LeftTableName = _GetTableAlias( leftMapping.ReferenceTable ),
						LeftPluralizedTableName = _GetPropertyName( usedPropertyNames, rightMapping.ReferenceTable, pluralizationService.Pluralize( _GetTableAlias( leftMapping.ReferenceTable ) ) ),
						LeftForeignKeyColumnNames = mCodeFirstDal.GetForeignKeyColumns( leftMapping.SourceTable, leftMapping.FKName ).Select( column => column.Item1 ).ToArray(),

						RightTableName = _GetTableAlias( rightMapping.ReferenceTable ),
						RightPluralizedTableName = _GetPropertyName( usedPropertyNames, leftMapping.ReferenceTable, pluralizationService.Pluralize( _GetTableAlias( rightMapping.ReferenceTable ) ) ),
						RightForeignKeyColumnNames = mCodeFirstDal.GetForeignKeyColumns( rightMapping.SourceTable, rightMapping.FKName ).Select( column => column.Item1 ).ToArray(),
					};

					manyManyRelationships.Add( manyManyRelationship );
				}

			}

			return manyManyRelationships;
		}

		public IEnumerable<string> GetPrimaryKeyColumns( string tableName )
		{
			return mCodeFirstDal.GetPrimaryKeys( tableName );
		}

		public IEnumerable<PrimitiveProperty> GetPrimitiveProperties( string tableName )
		{
			var primitiveProperties = mCodeFirstDal.GetPrimitivePropertiesForTable( tableName ).ToList();
			var primaryKeys = mCodeFirstDal.GetPrimaryKeys( tableName ).ToList();
			var singleNavigationProperties = GetSingleNavigationProperties( tableName );

			foreach( var navProperty in singleNavigationProperties )
			{
				int index = 0;
				foreach( var fkProperty in navProperty.ForeignKeyMappings )
				{
					var property =
						primitiveProperties.FirstOrDefault( prop => prop.PropertyName == fkProperty.ForeignKeyColumnName );

					if( property != null )
					{
						property.ForeignKeys.Add( new PrimativeForeignKeyFixup
						{
							NavPropertyName = navProperty.PropertyName,
							NavPropertyKeyName = fkProperty.PrimaryKeyColumnName
						} );
					}

					index++;
				}
			}

			foreach( var prop in primitiveProperties )
			{
				prop.IsPrimaryKey = primaryKeys.Contains( prop.PropertyName );
			}

			return primitiveProperties;
		}

		public IEnumerable<SingleNavigationProperty> GetSingleNavigationProperties( string tableName )
		{
			var mappings = mCodeFirstDal.GetTableMappingInfo( tableName );

			var navigationProperties = new List<SingleNavigationProperty>();
			var usedPropertyNames = new Dictionary<string, Dictionary<string, int>>();

			var pluralizationService = PluralizationService.CreateService( CultureInfo.CurrentCulture );

			// One To Many
			// One To One Dependent
			foreach( var singleRelationship in mappings.Where( map =>
				_DetermineRelationshipType( tableName, map ) == RelationshipType.OneToMany ||
				_DetermineRelationshipType( tableName, map ) == RelationshipType.OneToOneDependent ) )
			{
				var thisPropertyName = _GetPropertyName( usedPropertyNames, singleRelationship.SourceTable, _GetTableAlias( singleRelationship.ReferenceTable ) );
				string otherPropertyName;
				if( _DetermineRelationshipType( tableName, singleRelationship ) == RelationshipType.OneToMany )
				{
					otherPropertyName = _GetPropertyName( usedPropertyNames, singleRelationship.ReferenceTable, pluralizationService.Pluralize( _GetTableAlias( singleRelationship.SourceTable ) ) );
				}
				else
				{
					otherPropertyName = _GetPropertyName( usedPropertyNames, singleRelationship.ReferenceTable, _GetTableAlias( singleRelationship.SourceTable ) );
				}

				var navProperty = new SingleNavigationProperty
				{
					ReversePropertyName = otherPropertyName,
					PropertyType = _GetTableAlias( singleRelationship.ReferenceTable ),
					RelationshipType = _DetermineRelationshipType( tableName, singleRelationship ),
					IsCascadeDelete = singleRelationship.IsCascadeDelete,
					PropertyName = thisPropertyName,
					ForeignKeyMappings = mCodeFirstDal.GetForeignKeyMappings( singleRelationship.SourceTable, singleRelationship.FKName ),
					IsNullable = mCodeFirstDal.GetForeignKeyMappings( singleRelationship.SourceTable, singleRelationship.FKName ).All( map => map.IsForeignKeyNullable )
				};

				navigationProperties.Add( navProperty );
			}

			// One to One Principle
			foreach( var singleRelationship in mappings.Where( map =>
				_DetermineRelationshipType( tableName, map ) == RelationshipType.OneToOnePrinciple ) )
			{
				var thisPropertyName = _GetPropertyName( usedPropertyNames, singleRelationship.ReferenceTable, _GetTableAlias( singleRelationship.SourceTable ) );
				var otherPropertyName = _GetPropertyName( usedPropertyNames, singleRelationship.SourceTable, _GetTableAlias( singleRelationship.ReferenceTable ) );

				var navProperty = new SingleNavigationProperty
				{
					ReversePropertyName = otherPropertyName,
					PropertyType = _GetTableAlias( singleRelationship.SourceTable ),
					RelationshipType = RelationshipType.OneToOnePrinciple,
					IsCascadeDelete = singleRelationship.IsCascadeDelete,
					PropertyName = thisPropertyName,
					ForeignKeyMappings = mCodeFirstDal.GetForeignKeyMappings( singleRelationship.SourceTable, singleRelationship.FKName ),
					IsNullable = mCodeFirstDal.GetForeignKeyMappings( singleRelationship.SourceTable, singleRelationship.FKName ).All( map => map.IsForeignKeyNullable )
				};

				navigationProperties.Add( navProperty );
			}

			return navigationProperties;
		}

		public bool IsManyToManyMappingTable( string tableName )
		{
			return mManyToManyMappingTablesCache.Contains( tableName );
		}

		public static CodeFirstGenerator Load( string connectionString, string[][] tableAndAliases )
		{
			return new CodeFirstGenerator( connectionString, tableAndAliases );
		}

		public static CodeFirstGenerator Load( string connectionString, Dictionary<string,string> tableAndAliases )
		{
			return new CodeFirstGenerator( connectionString, tableAndAliases );
		}

		#endregion CodeFirstGenerator Members

		#region Fields

		private readonly CodeFirstDal mCodeFirstDal;
		private readonly Dictionary<string, string> mTablesAndAliasesCache;
		private readonly HashSet<string> mManyToManyMappingTablesCache;

		#endregion Fields

		#region Private Members

		private RelationshipType _DetermineRelationshipType( string tableToGenerate, TableMappingInfo mapping )
		{
			RelationshipType relationshipType;

			if( string.Equals( mapping.ReferenceTable, tableToGenerate, StringComparison.OrdinalIgnoreCase ) )
			{
				if( mManyToManyMappingTablesCache.Contains( mapping.SourceTable ) )
				{
					relationshipType = RelationshipType.ManyToMany;
				}
				else if( !mapping.OneToOneRelationship )
				{
					relationshipType = RelationshipType.ManyToOne;
				}
				else
				{
					relationshipType = RelationshipType.OneToOnePrinciple;
				}
			}
			else if( string.Equals( mapping.SourceTable, tableToGenerate, StringComparison.OrdinalIgnoreCase ) )
			{
				relationshipType = mapping.OneToOneRelationship
					? RelationshipType.OneToOneDependent
					: RelationshipType.OneToMany;
			}
			else
			{
				throw new InvalidOperationException( "This should never happen" );
			}

			return relationshipType;
		}

		private string _GetPropertyName( Dictionary<string, Dictionary<string, int>> usedPropertyNames, string tableName, string navigationPropertyName )
		{
			Dictionary<string, int> propertyCountLookup;
			if( usedPropertyNames.TryGetValue( tableName, out propertyCountLookup ) )
			{
				int propertyCount;
				if( propertyCountLookup.TryGetValue( navigationPropertyName, out propertyCount ) )
				{
					propertyCount++;
					propertyCountLookup[navigationPropertyName] = propertyCount;
					navigationPropertyName = navigationPropertyName + propertyCount;
				}
				else
				{
					propertyCountLookup.Add( navigationPropertyName, 0 );
				}
			}
			else
			{
				usedPropertyNames.Add( tableName, new Dictionary<string, int>
				{
					{ navigationPropertyName, 0 }
				} );
			}

			return navigationPropertyName;
		}

		private string _GetTableAlias( string tableName )
		{
			string alias;
			if( mTablesAndAliasesCache.ContainsKey( tableName ) )
			{
				alias = mTablesAndAliasesCache[tableName];
			}
			else
			{
				alias = tableName;
			}
			return alias;
		}

		#endregion Private Members

	}
}
