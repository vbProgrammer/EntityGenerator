using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace CodeGenerators.CodeFirst
{
	public class CodeFirstDal
	{
		#region Constructors

		public CodeFirstDal( string connectionString )
		{
			mConnectionString = connectionString;
		}

		#endregion Constructors

		#region CodeFirstDal Members

		public IEnumerable<ColumnMappingInfo> GetColumnMappingInfos( string tableName )
		{
			var sqlReader = _GetSqlDataReaderFromSqlCommand(
				string.Format(
					@"SELECT	 TABLE_NAME				AS [TableName]
								,COLUMN_NAME			AS [ColumnName]  
								,DATA_TYPE				AS [Type]
								,CASE 
									WHEN COLUMNPROPERTY( OBJECT_ID( TABLE_SCHEMA +'.'+ TABLE_NAME ),COLUMN_NAME,'IsIdentity') = 1 
									THEN 1 
									WHEN DATA_TYPE = 'uniqueidentifier'
									AND COLUMN_DEFAULT IS NOT NULL
									THEN 1
									ELSE 0 END AS [IsStorageGenerated]
								,IS_NULLABLE			AS [IsNullable]
								,CASE DATA_TYPE 
									WHEN 'nvarchar' THEN 1
									WHEN 'nchar' THEN 1 
									ELSE 0 END			AS [IsUnicode]
								,CASE DATA_TYPE 
									WHEN 'char' THEN 1 
									WHEN 'nchar' THEN 1
									ELSE 0 END			AS [IsFixedLength]
								,CHARACTER_MAXIMUM_LENGTH AS [MaxLength]
								,CASE DATA_TYPE 
									WHEN 'timestamp' THEN 1 
									ELSE 0 END			AS [IsRowVersion],
								ISNULL((SELECT 1
										FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE keyUsage
										WHERE OBJECTPROPERTY(OBJECT_ID(keyUsage.constraint_name), 'IsPrimaryKey') = 1
										AND keyUsage.TABLE_NAME = cols.TABLE_NAME 
										AND keyUsage.COLUMN_NAME = cols.COLUMN_NAME ), 0) AS [IsPrimaryKey]
								,NUMERIC_PRECISION		AS [Precision]
								,NUMERIC_SCALE			AS [Scale]
						FROM INFORMATION_SCHEMA.COLUMNS cols
						WHERE TABLE_NAME = '{0}'",
					tableName ) );

			var columnMappingInfos = new List<ColumnMappingInfo>();

			while( sqlReader.Read() )
			{
				var tableNameResult = sqlReader.GetString( 0 );
				var columnName = sqlReader.GetString( 1 );
				var sqlDbType = sqlReader.GetString( 2 );
				var mappingInfoType = _GetTypeStringFromSqlDbTypeString( sqlDbType, false );
				var isStorageGenerated = sqlReader.GetInt32( 3 ) == 1;
				var isNullable = sqlReader.GetString( 4 ) == "YES";
				var isUnicode = sqlReader.GetInt32( 5 ) == 1 || sqlDbType.ToLower() == "xml";
				var isFixedLength = sqlReader.GetInt32( 6 ) == 1;
				var maxLength = sqlReader.IsDBNull( 7 )
					? new int?()
					: sqlReader.GetInt32( 7 );
				var isRowVersion = sqlReader.GetInt32( 8 ) == 1;
				var isPrimaryKey = sqlReader.GetInt32( 9 ) == 1;
				var precision = sqlReader.IsDBNull( 10 )
					? new byte?()
					: sqlReader.GetByte( 10 );
				var scale = sqlReader.IsDBNull( 11 )
					? new int?()
					: sqlReader.GetInt32( 11 );

				var mappingInfo = new ColumnMappingInfo
				{
					TableName = tableNameResult,
					ColumnName = columnName,
					Type = mappingInfoType,
					IsStorageGenerated = isStorageGenerated,
					IsNullable = isNullable,
					IsUnicode = isUnicode,
					IsFixedLength = isFixedLength,
					MaxLength = maxLength,
					IsRowVersion = isRowVersion,
					IsPrimaryKey = isPrimaryKey,
					Precision = precision,
					Scale = scale
				};
				columnMappingInfos.Add( mappingInfo );
			}

			return columnMappingInfos
				.OrderBy( colMap => colMap.TableName )
				.ThenBy( colMap => colMap.ColumnName );

		}

		public IEnumerable<Tuple<string, bool>> GetForeignKeyColumns( string tableName, string foreignKeyName )
		{
			var sqlReader = _GetSqlDataReaderFromSqlCommand(
				string.Format(
					@"SELECT usage.COLUMN_NAME AS [ColumnName],
							COLUMNPROPERTY( OBJECT_ID( TABLE_SCHEMA +'.'+ TABLE_NAME ),COLUMN_NAME,'AllowsNull') AS [IsNullable]
					FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE usage 
					WHERE usage.TABLE_NAME = '{0}'
					AND usage.CONSTRAINT_NAME = '{1}'",
					tableName,
					foreignKeyName ) );

			var fkColumns = new List<Tuple<string, bool>>();

			while( sqlReader.Read() )
			{
				var columnName = sqlReader.GetString( 0 );
				var isNullable = sqlReader.GetInt32( 1 ) == 1;

				fkColumns.Add( new Tuple<string, bool>( columnName, isNullable ) );
			}
			return fkColumns.OrderBy( col => col.Item1 );
		}

		public IEnumerable<ForeignKeyMapping> GetForeignKeyMappings( string tableName, string foreignKeyName )
		{
			var sqlReader = _GetSqlDataReaderFromSqlCommand(
				string.Format(
					@"SELECT fkCols.name		AS [ForeignKeyColumnName]
							,pkCols.name		AS [PrimaryKeyColumnName]
							,fkCols.is_nullable AS [IsNullable]

						FROM sys.foreign_key_columns keyColumns
						JOIN sys.columns fkCols
							ON fkCols.object_id = keyColumns.parent_object_id 
							AND fkCols.column_id = keyColumns.parent_column_id
						JOIN sys.columns pkCols
							ON pkCols.object_id = keyColumns.referenced_object_id 
							AND pkCols.column_id = keyColumns.referenced_column_id

						WHERE OBJECT_NAME(keyColumns.parent_object_id) = '{0}'
							  AND OBJECT_NAME(keyColumns.constraint_object_id) = '{1}'",
					tableName,
					foreignKeyName ) );

			var fkColumns = new List<ForeignKeyMapping>();

			while( sqlReader.Read() )
			{
				fkColumns.Add( new ForeignKeyMapping
				{
					ForeignKeyColumnName = sqlReader.GetString( 0 ),
					PrimaryKeyColumnName = sqlReader.GetString( 1 ),
					IsForeignKeyNullable = sqlReader.GetBoolean( 2 )
				} );
			}
			return fkColumns.OrderBy( col => col.ForeignKeyColumnName );
		}

		public IEnumerable<string> GetManyToManyMappingTables()
		{
			var sqlReader = _GetSqlDataReaderFromSqlCommand(
			"SELECT fkCount.TABLE_NAME FROM " +
			"(SELECT tables.TABLE_NAME FROM INFORMATION_SCHEMA.TABLES tables  " +
			"WHERE ((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS cols WHERE tables.TABLE_NAME = cols.TABLE_NAME ) =  " +
			"(SELECT COUNT(*) FROM  (SELECT keyCols.COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE keyCols WHERE keyCols.TABLE_NAME = Tables.TABLE_NAME " +
			"AND OBJECTPROPERTY(OBJECT_ID(keyCols.CONSTRAINT_NAME), 'IsForeignKey') = 1) fkColCount))) coveredTables " +
			"JOIN (SELECT TABLE_NAME, count(*) tableFKCount FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc " +
			"WHERE tc.CONSTRAINT_TYPE = 'FOREIGN KEY' " +
			"group by TABLE_NAME) fkCount " +
			"on coveredTables.TABLE_NAME = fkCount.TABLE_NAME " +
			"where tableFKCount = 2" );
			var manyToManyTables = new List<string>();
			while( sqlReader.Read() )
			{
				manyToManyTables.Add( sqlReader.GetString( 0 ) );
			}
			return manyToManyTables.OrderBy( map => map );
		}

		public IEnumerable<string> GetPrimaryKeys( string tableName )
		{
			var sqlReader = _GetSqlDataReaderFromSqlCommand(
				string.Format(
					@"SELECT COLUMN_NAME
					FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
					WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_NAME), 'IsPrimaryKey') = 1
					AND TABLE_NAME = '{0}'",
					tableName ) );
			var primaryKeyColumns = new List<string>();

			while( sqlReader.Read() )
			{
				primaryKeyColumns.Add( sqlReader.GetString( 0 ) );
			}

			return primaryKeyColumns.OrderBy( col => col );
		}

		public IEnumerable<PrimitiveProperty> GetPrimitivePropertiesForTable( string tableToGenerate )
		{
			var sqlDataReader = _GetSqlDataReaderFromSqlCommand(
				string.Format(
					"SELECT allColumns.COLUMN_NAME, " +
						"		allColumns.DATA_TYPE, " +
						"       allColumns.IS_NULLABLE " +
						"FROM INFORMATION_SCHEMA.COLUMNS allColumns " +
						"WHERE allColumns.table_name = '{0}' ",
					tableToGenerate ) );

			var primitiveProperties = new List<PrimitiveProperty>();
			while( sqlDataReader.Read() )
			{
				var propertyName = sqlDataReader.GetString( 0 );
				var propertyType = _GetTypeStringFromSqlDbTypeString( sqlDataReader.GetString( 1 ), sqlDataReader.GetString( 2 ) == "YES" );
				primitiveProperties.Add( new PrimitiveProperty
				{
					PropertyName = propertyName,
					PropertyType = propertyType
				} );
			}
			sqlDataReader.Close();
			return primitiveProperties.OrderBy( prop => prop.PropertyName );
		}

		public IEnumerable<TableMappingInfo> GetTableMappingInfo( string tableName )
		{
			var sqlReader = _GetSqlDataReaderFromSqlCommand(
				string.Format(
					@"SELECT  colUsage.TABLE_NAME						AS [SourceTable],
							colUsage.CONSTRAINT_NAME					AS [FKName],
							refColUsage.TABLE_NAME						AS [ReferenceTable],
							CASE refConstraints.DELETE_RULE
								WHEN 'CASCADE' THEN 1
								ELSE 0
							END											AS [IsCascadeDelete],
							ISNULL((SELECT 1 FROM (SELECT (SELECT COUNT(*)
					 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE pkCols
					 WHERE pkCols.TABLE_NAME = colUsage.TABLE_NAME
						AND OBJECTPROPERTY(OBJECT_ID(pkCols.CONSTRAINT_NAME), 'IsPrimaryKey') = 1 ) AS [pkCount],
	
					(SELECT COUNT(*)
					 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE pkCols
					 JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE fkCols 
					 ON pkCols.COLUMN_NAME = fkCols.COLUMN_NAME 
						AND pkCols.TABLE_NAME = fkCols.TABLE_NAME
						AND fkCols.CONSTRAINT_NAME = colUsage.CONSTRAINT_NAME
					 WHERE pkCols.TABLE_NAME = colUsage.TABLE_NAME
						AND OBJECTPROPERTY(OBJECT_ID(pkCols.CONSTRAINT_NAME), 'IsPrimaryKey') = 1 ) AS [fkPkCount] ,
	
					(SELECT COUNT(*)
					 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE fkCols
					 WHERE fkCols.TABLE_NAME = colUsage.TABLE_NAME
					 AND fkCols.CONSTRAINT_NAME = colUsage.CONSTRAINT_NAME) AS [fkCounts] ) counts
					 WHERE pkCount = fkPkCount AND fkCounts = fkPkCount), 0) AS [OneToOneRelationship]

					FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS colUsage

					JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS refConstraints ON refConstraints.CONSTRAINT_NAME = colUsage.CONSTRAINT_NAME 
					JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS refColUsage ON refColUsage.CONSTRAINT_NAME = refConstraints.UNIQUE_CONSTRAINT_NAME

					WHERE colUsage.TABLE_NAME = '{0}' 
					OR refColUsage.TABLE_NAME = '{0}'",
					tableName ) );

			var tableMappings = new List<TableMappingInfo>();
			while( sqlReader.Read() )
			{
				var mappingInfo = new TableMappingInfo
				{
					SourceTable = sqlReader.GetString( 0 ),
					FKName = sqlReader.GetString( 1 ),
					ReferenceTable = sqlReader.GetString( 2 ),
					IsCascadeDelete = sqlReader.GetInt32( 3 ) == 1,
					OneToOneRelationship = sqlReader.GetInt32( 4 ) == 1
				};
				tableMappings.Add( mappingInfo );
			}

			return tableMappings
				.OrderBy( map => map.SourceTable )
				.ThenBy( map => map.ReferenceTable );
		}

		#endregion CodeFirstDal Members

		#region Fields

		private string mConnectionString;

		#endregion Fields

		#region Private Members

		private SqlDataReader _GetSqlDataReaderFromSqlCommand( string commandText )
		{
			var sqlConnection = new SqlConnection( mConnectionString );
			var sqlCommand = new SqlCommand
			{
				CommandText = commandText,
				CommandType = CommandType.Text,
				Connection = sqlConnection
			};
			sqlConnection.Open();
			return sqlCommand.ExecuteReader( CommandBehavior.CloseConnection );
		}

		private string _GetTypeStringFromSqlDbTypeString( string sqlDbTypeString, bool nullable )
		{

			string typeString;
			switch( sqlDbTypeString.ToLower() )
			{
				case "bigint":
					typeString = "long";
					break;

				case "binary":
				case "image":
				case "timestamp":
				case "varbinary":
					typeString = "byte[]";
					break;

				case "bit":
					typeString = "bool";
					break;

				case "char":
				case "nchar":
				case "ntext":
				case "nvarchar":
				case "text":
				case "varchar":
				case "xml":
					typeString = "string";
					break;

				case "datetime":
				case "smalldatetime":
				case "date":
				case "time":
				case "datetime2":
					typeString = "DateTime";
					break;

				case "decimal":
				case "money":
				case "numeric":
				case "smallmoney":
					typeString = "decimal";
					break;

				case "float":
					typeString = "double";
					break;

				case "int":
					typeString = "int";
					break;

				case "real":
					typeString = "float";
					break;

				case "uniqueidentifier":
					typeString = "Guid";
					break;

				case "smallint":
					typeString = "short";
					break;

				case "tinyint":
					typeString = "byte";
					break;

				case "variant":
				case "udt":
					typeString = "object";
					break;

				case "structured":
					typeString = "DataTable";
					break;

				case "datetimeoffset":
					typeString = "DateTimeOffset";
					break;

				default:
					throw new ArgumentOutOfRangeException( sqlDbTypeString );
			}

			if( nullable &&
				!typeString.Equals( "string" ) &&
				!typeString.Equals( "byte[]" ) &&
				!typeString.Equals( "object" ) )
			{
				typeString = string.Format( "{0}?", typeString );
			}

			return typeString;
		}

		#endregion Private Members

	}
}
