using System.Text.RegularExpressions;
using ApiInterface.Structures;
using ApiInterface.Models;
using System.Data;
using System.Text.Json;
using System.IO;
using System.Data.Common;
using System.Runtime.CompilerServices;
namespace ApiInterface.Store
{
  /*
   * Esta clase guarda los datos en memoria, donde Table es la tabla. 
   */
  internal class Data
  {
    private string? FileData;
    private string? Name;
    private DataPath? Path = null;
    public Table? Table = null;
    public string Database;

    public void CreateTableAs(string name, string[] fields)
    {
      Name = name;
      LinkedList<Field> headers = new LinkedList<Field>();


      foreach (string fieldDef in fields)
      {
        string[] parts = fieldDef.Trim().Split(' ');
        if (parts.Length < 2)
        {
          throw new ArgumentException($"Definición de campo inválida: {fieldDef}");
        }

        string fieldName = parts[0];
        string fieldType = parts[1].ToUpper();
        int? fieldSize = 32;

        if (fieldType.Contains("VARCHAR"))
        {
          string[] sizeParts = fieldType.Split('(');

          if (sizeParts.Length > 1){
            fieldSize = int.Parse(sizeParts[1].Trim(')', ' '));
          }
        } 
        headers.AddLast(new Field(fieldName, fieldType, fieldSize, "HEAD"));
        Console.WriteLine($"Field: {fieldName} - Type: {fieldType} - Size: {fieldSize} COUNT {fields.Length}");
      }
      Table newTable = new Table(name, new List<List<Field>>(), headers);
      Table = newTable;
    }

    public void CreateDatabase(string databaseName)
    {
      if (string.IsNullOrWhiteSpace(databaseName))
      {
        throw new ArgumentException("El nombre de la base de datos no puede estar vacío.");
      }

      Console.WriteLine($"CREATE DATABASE {databaseName}");

      // Actualizar el path de la base de datos
      Path = new DataPath(databaseName);

      Console.WriteLine($"Base de datos '{databaseName}' creada exitosamente.");
    }

    public void SetDatabase(string databaseName)
    {
      if (string.IsNullOrWhiteSpace(databaseName))
      {
        throw new ArgumentException("El nombre de la base de datos no puede estar vacío.");
      }

      Database = databaseName;
      Path = new DataPath(databaseName);
      Console.WriteLine($"Contexto establecido a la base de datos '{databaseName}'.");
    }

    public void DropTable(string tableName)
    {
      Console.WriteLine($"DROP TABLE {tableName} | Database: {Database}");
      Path = new DataPath(Database);
      if (string.IsNullOrWhiteSpace(tableName))
      {
        throw new ArgumentException("El nombre de la tabla no puede estar vacío.");
      }
      Path.Drop(tableName);
      Table = null;
      Console.WriteLine($"Tabla '{tableName}' eliminada exitosamente de la base de datos '{Database}'.");
    }
    public void Select(
        string tableName,
        string selectedColumn,
        string? whereClause = null,
        string? likeWord = null,
        string? orderBy = null,
        bool isAscending = true,
        string? whereValue = null
        )
    {
      List<string> result = new List<string>();

      if (whereClause.Contains("index"))
      {
        int index = 0;
        string pattern = @"index\s*=\s*(\d+)";
        Match match = Regex.Match(whereClause, pattern);
        if (match.Success)
        {
          index = int.Parse(match.Groups[1].Value);
        }
        if (index > 0 && index < Table.TableFields.Count)
        {
          List<Field> row = Table.FindById(index);
          foreach (Field field in row)
          {
            if (field.Name == selectedColumn || selectedColumn == "*")
            {
              result.Add(field.Value.ToString());
              break;
            }
          }
        }
      }

      if (likeWord != null && whereClause != null)
      {
        Match match = Regex.Match(whereClause, @"(\w+)\s*LIKE\s*(\w+)");
        if (!match.Success)
        {
          throw new ArgumentException("La cláusula WHERE debe contener un operador de comparación.");
        }

        string columnName = match.Groups[1].Value;
        string value = match.Groups[2].Value;

        Match likeMatch = Regex.Match(whereClause, @"LIKE\s+\*(\w+)\*");
        if (likeMatch.Success)
        {
          likeWord = likeMatch.Groups[1].Value;
        }

        foreach (List<Field> row in Table.TableFields)
        {
          foreach (Field field in row)
          {
            if (field.Name == columnName && field.Value.ToString().Contains(likeWord))
            {
              foreach (Field innerField in row)
              {
                if (innerField.Name == selectedColumn || selectedColumn == "*")
                {
                  result.Add(innerField.Value.ToString());
                }
              }
            }
          }
        }

        if (orderBy != null)
        {
          result = OrderBy(result, orderBy, isAscending);
        }
        Console.WriteLine($"Result: {string.Join(", ", result)}");
      }
      else if (whereClause != null && whereValue != null)
      {
        Console.WriteLine($"WHERE: {whereClause}");
        Match match = Regex.Match(whereClause, @"(\w+)\s*\=\s*(\w+)");
        if (!match.Success)
        {
          throw new ArgumentException("La cláusula WHERE debe contener un operador de comparación.");
        }

        string columnName = match.Groups[1].Value.Replace(" ", "");
        string value = whereValue.Trim('\'', '\"').Replace(" ", "");

        foreach (List<Field> row in Table.TableFields)
        {
          foreach (Field field in row)
          {
            if (field.Name.Contains(columnName) && field.Value.ToString().Contains(value))
            {
              foreach (Field innerField in row)
              {
                if (innerField.Name == selectedColumn || selectedColumn == "*")
                {
                  result.Add(innerField.Value.ToString());
                }
              }
            }
          }
        }

        if (orderBy != null)
        {
          result = OrderBy(result, orderBy, isAscending);
        }

      }
      Console.WriteLine($"Result: {string.Join(", ", result)}");
    }

    private List<string> OrderBy(List<string> result, string orderBy, bool isAscending)
    {
      var ordered = result.OrderBy(item =>
      {
        // Intenta parsear el item como número
        if (int.TryParse(item.ToString(), out int number))
        {
          return number;
        }
        // Si no es un número, trata el item como string
        return int.MaxValue; // Para elementos no numéricos
      })
      .ThenBy(item => item); // Asegura un orden consistente para items con el mismo valor numérico

      // Aplicar el orden ascendente o descendente
      return isAscending ? ordered.ToList() : ordered.Reverse().ToList();
    }

    public void CreateIndex(string indexName, string tableName, string column)
    {
      if (Table == null)
      {
        throw new ArgumentException($"La tabla '{tableName}' no existe.");
      }

      if (Table.HasIndex)
      {
        throw new ArgumentException($"La tabla '{tableName}' ya tiene un índice.");
      }

      TableIndex tableIndex = TableIndex.BTree;

      if (indexName == "BSTREE")
      {
        tableIndex = TableIndex.BSTree;
      }

      Path.AddIndex(tableName, indexName, tableIndex.ToString());
      Table.CreateIndex(tableIndex, column);
    }


    public void Update(string tableName, string column, string value, string whereColumn, string whereValue)
    {
      // Verificar si la tabla existe
      if (Table == null)
      {
        throw new ArgumentException($"La tabla '{tableName}' no existe.");
      }

      Console.WriteLine($"INSIDE: UPDATE {tableName} SET {column} = {value} WHERE {whereColumn} = {whereValue}");

      Table.Update(column, value, whereColumn, whereValue);

      // Actualiza FileData 
      FileData = "";
      foreach (List<Field> row in Table.TableFields)
      {
        foreach (Field field in row)
        {
          FileData += field.Value + "\t";
        }
        FileData += "\n";
      }
      Console.WriteLine(FileData);
    }

    public void Save()
    {
      if (Table != null && Path != null)
      {
        Path.SaveTableAs(Table.Name, Table, FileData);
      }
      FileData = "";
    }

    public void Delete(string tableName, string? whereClause = null)
    {
      Console.WriteLine($" INNER DELETE FROM {tableName} WHERE {whereClause}");
      // Verificar si la tabla existe
      if (Table == null)
      {
        throw new ArgumentException($"La tabla '{tableName}' no existe.");
      }

      if (string.IsNullOrEmpty(whereClause))
      {
        throw new ArgumentException("La cláusula WHERE es requerida para eliminar filas.");
      }

      Match match = Regex.Match(whereClause, @"(\w+)\s*=\s*'?([^']+)'?");
      if (!match.Success)
      {
        throw new ArgumentException("La cláusula WHERE debe contener un operador de comparación.");
      }

      string columnName = match.Groups[1].Value;
      string value = match.Groups[2].Value.Replace("'", "").Replace("\"", "");
      Console.WriteLine($" FROM {tableName} WHERE {columnName} = {value}");

      List<List<Field>> rowsToRemove = new List<List<Field>>();
      foreach (List<Field> row in Table.TableFields)
      {
        if (row.Any(field => (field.Value.Equals(value) || field.Value.ToString().Contains(value) ) && field.Name == columnName))
        {
          rowsToRemove.Add(row);
        }
      }

      foreach (List<Field> row in rowsToRemove)
      {
        Table.TableFields.Remove(row);
      }

      // Actualiza FileData
      FileData = "";
      foreach (List<Field> row in Table.TableFields)
      {
        foreach (Field field in row)
        {
          FileData += field.Value + "\t";
        }
        FileData += "\n";
      }
      Console.WriteLine(FileData);

    }

    public void Insert(string tableName, string[] values)
    {
      if (Table == null)
      {
        throw new ArgumentException($"La tabla '{tableName}' no existe.");
      }

      List<Field> fields = new List<Field>();
      for (int i = 0; i < Table.Headers.Count; i++)
      {
        FileData += values[i] + "\t";
        Field field = Table.Headers.ElementAt(i);
        // Removes the quotes from the value
        string value = values[i].Replace("\"", "").Replace("'", "");
        fields.Add(new Field(field.Name, field.Type, field.Size, value));
      }
      FileData += "\n"; 
      if (Table.HasIndex){
        Table.Insert(int.Parse(values[0]), fields);
      } else {
        Table.TableFields.Add(fields);
      }
      Console.WriteLine($"agregando en: {tableName} VALUES {string.Join(", ", values)} | Filas: {Table.TableFields.Count}");
    }

    public void SetTableAs(string name)
    {
      Table? table = Path.LoadTable(name);
      if (table != null)
      {
        Table = table;
      }
    }
    public void SetDatabaseAs(string name)
    {
      Path = new(name);
    }
  }


  /*
   * Esta clase maneja los paths del proyecto
   */
  internal class DataPath
  {
    private string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private string Database;
    private string SystemCatalogPath;
    private string? TableName;

    public DataPath(string Name)
    {
        Database = Name;
        string databasePath = Path.Combine(AppDataPath, "DatabaseApp", Database);
        SystemCatalogPath = Path.Combine(AppDataPath, "DatabaseApp", "SystemCatalog");
        Directory.CreateDirectory(databasePath);
        Directory.CreateDirectory(SystemCatalogPath);
        InitializeSystemCatalog();
        AddDatabase(Database);
    }

    private void InitializeSystemCatalog()
    {
        string[] catalogFiles = { "SystemDatabases", "SystemTables", "SystemColumns", "SystemIndexes" };
        foreach (var file in catalogFiles)
        {
            string filePath = Path.Combine(SystemCatalogPath, file);
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
        }
    }

    public void AddDatabase(string databaseName)
    {
        string filePath = Path.Combine(SystemCatalogPath, "SystemDatabases");
        using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Append)))
        {
            writer.Write(databaseName);
        }
    }

    public void AddTable(string tableName)
    {
        string filePath = Path.Combine(SystemCatalogPath, "SystemTables");
        using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Append)))
        {
            writer.Write(Database);
            writer.Write(tableName);
        }
    }

    public void AddIndex(string tableName, string indexName, string indexType)
    {
        string filePath = Path.Combine(SystemCatalogPath, "SystemIndexes");
        using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Append)))
        {
            writer.Write(Database);
            writer.Write(tableName);
            writer.Write(indexName);
            writer.Write(indexType);
        }
    }

    public List<string> GetDatabases()
    {
        string filePath = Path.Combine(SystemCatalogPath, "SystemDatabases");
        List<string> databases = new List<string>();
        using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                databases.Add(reader.ReadString());
            }
        }
        return databases;
    }

    public List<string> GetTables()
    {
        string filePath = Path.Combine(SystemCatalogPath, "SystemTables");
        List<string> tables = new List<string>();
        using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                string db = reader.ReadString();
                string table = reader.ReadString();
                if (db == Database)
                {
                    tables.Add(table);
                }
            }
        }
        return tables;
    }

    public void SaveTableAs(string name, Table data, string FileData)
    {
      try
      {
        TableName = name;
        string filePath = GetTableFilePath();
        Console.WriteLine($"Saving table to {filePath}");
        // Asegúrate de que el directorio existe
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        string jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        File.WriteAllText(filePath + ".json", jsonData);

        if (File.Exists(filePath + ".json"))
        {
            Console.WriteLine($"Tabla '{name}' guardada exitosamente en {filePath}");
        }
        else
        {
            Console.WriteLine($"Error: No se pudo verificar la existencia del archivo después de guardarlo.");
        }


        // Opens another file called TableName.table and writes the FileData
        string tableFile = Path.Combine(Path.GetDirectoryName(filePath + ".json"), $"{name}.table");
        File.WriteAllText(tableFile, FileData);
        AddTable(name);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error al guardar la tabla '{name}': {ex.Message}");
      }
    }

    public Table? LoadTable(string name)
    {
      TableName = name;
      string filePath = GetTableFilePath();
      if (!File.Exists(filePath + ".json"))
      {
        Console.WriteLine($"La tabla '{name}' no existe en la base de datos '{Database}'.");
        return null;
      }

      string jsonData = File.ReadAllText(filePath + ".json");
      Table? loadedData = JsonSerializer.Deserialize<Table>(jsonData);
      Console.WriteLine($"Tabla '{name}' cargada exitosamente desde {filePath}");
      return loadedData;
    }

    public void Drop(string name)
    {
      TableName = name;
      string path = GetTableFilePath();
      File.Delete(path + ".json");
      File.Delete(path + ".table");
    }

    private string GetTableFilePath()
    {
      if (string.IsNullOrEmpty(TableName))
      {
        throw new InvalidOperationException("Nombre de tabla no especificado.");
      }
      return Path.Combine(AppDataPath, "DatabaseApp", Database, $"{TableName}");
    }
  }

}
