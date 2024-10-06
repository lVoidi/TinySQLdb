using ApiInterface.Structures;
using System.Data;
using System.Text.Json;
using System.IO;
namespace ApiInterface.Store
{
  /*
   * Esta clase guarda los datos en memoria, donde Table es la tabla. 
   */
  internal class Data
  {
    private string? FileData;
    private string? Name;
    List<Dictionary<string, object>> Selected = new();
    private DataPath? Path;
    private List<Table> Tables = new List<Table>();

    public void CreateTableAs(string name, string fields)
    {
      Name = name;
      string[] fieldDefinitions = fields.Split(',');
      List<Field> tableFields = new List<Field>();


      foreach (string fieldDef in fieldDefinitions)
      {
        string[] parts = fieldDef.Trim().Split(' ');
        if (parts.Length < 2)
        {
          throw new ArgumentException($"Definición de campo inválida: {fieldDef}");
        }

        string fieldName = parts[0];
        string fieldType = parts[1].ToUpper();
        int? fieldSize = null;

        switch (fieldType)
        {
          case "INTEGER":
          case "DOUBLE":
          case "DATETIME":
            break;
          case "VARCHAR":
            if (parts.Length < 3 || !int.TryParse(parts[2].Trim('(', ')'), out int size))
            {
              throw new ArgumentException($"El tipo VARCHAR debe especificar un tamaño válido: {fieldDef}");
            }
            fieldSize = size;
            break;
          default:
            throw new ArgumentException($"Tipo de dato no soportado: {fieldType}");
        }

        tableFields.Add(new Field(fieldName, fieldType, fieldSize));
      }

      // Crear una nueva tabla y añadirla a la lista de tablas
      Table newTable = new Table(name, tableFields);
      Tables.Add(newTable);

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

      // Aquí deberíamos verificar si la base de datos existe
      // Por ahora, simularemos esta verificación
      if (!DatabaseExists(databaseName))
      {
        throw new InvalidOperationException($"La base de datos '{databaseName}' no existe.");
      }

      Database = databaseName;
      Console.WriteLine($"Contexto establecido a la base de datos '{databaseName}'.");
    }

    // Esta funcion compruba que una base de datos existe en el 
    // Sistema de archivos. Simplemente es comprobar que databaseName existe en 
    // el DataPath
    private bool DatabaseExists(string databaseName)
    {
      return true;
    }

    public string Database { get; private set; } = string.Empty;

    public void DropTableIfEmpty(string tableName)
    {
      if (string.IsNullOrWhiteSpace(tableName))
      {
        throw new ArgumentException("El nombre de la tabla no puede estar vacío.");
      }

      if (string.IsNullOrWhiteSpace(Database))
      {
        throw new InvalidOperationException("No se ha establecido una base de datos de contexto.");
      }

      // Verificar si la tabla existe
      if (!TableExists(tableName))
      {
        throw new InvalidOperationException($"La tabla '{tableName}' no existe en la base de datos '{Database}'.");
      }

      // Verificar si la tabla está vacía
      if (IsTableEmpty(tableName))
      {
        // Eliminar la tabla
        Console.WriteLine($"DROP TABLE {tableName}");
        // Aquí iría la lógica real para eliminar la tabla
        Console.WriteLine($"Tabla '{tableName}' eliminada exitosamente de la base de datos '{Database}'.");
      }
      else
      {
        Console.WriteLine($"La tabla '{tableName}' no está vacía y no se eliminará.");
      }
    }

    private bool TableExists(string tableName)
    {
      foreach (Table table in Tables)
      {
        if (table.Name == tableName)
        {
          return true;
        }
      }
      return false;
    }

    private bool IsTableEmpty(string tableName)
    {
      foreach (Table table in Tables)
      {
        if (table.Name == tableName)
        {
          return table.TableFields.Count == 0;
        }
      }
      return false;
    }

    public List<Dictionary<string, object>> Select(
        string tableName,
        List<string> columns,
        string? whereClause = null,
        string? orderBy = null,
        bool isAscending = true)
    {
      // Verificar si la tabla existe
      Table? table = Tables.Find(t => t.Name == tableName);
      if (table == null)
      {
        throw new ArgumentException($"La tabla '{tableName}' no existe.");
      }

      List<Dictionary<string, object>> rows = GetTableRows(table);

      // Aplicar la cláusula WHERE si existe
      if (!string.IsNullOrEmpty(whereClause))
      {
        rows = ApplyWhereClause(rows, whereClause, table);
      }

      // Aplicar ORDER BY si se especifica
      if (!string.IsNullOrEmpty(orderBy))
      {
        ApplyOrderBy(rows, orderBy, isAscending);
      }

      // Seleccionar las columnas especificadas
      if (columns.Count > 0 && columns[0] != "*")
      {
        rows = SelectColumns(rows, columns);
      }

      return rows;
    }

    private List<Dictionary<string, object>> GetTableRows(Table table)
    {
      List<Dictionary<string, object>> tableRows = new();

      foreach (Field row in table.TableFields)
      {

        Dictionary<string, object> rowDictionary = new();
        rowDictionary.Add(row.Name, row.Type);
        tableRows.Add(rowDictionary);
      }
      return tableRows;
    }

    private List<Dictionary<string, object>> ApplyWhereClause(List<Dictionary<string, object>> rows, string whereClause, Table table)
    {
      // Parsear la cláusula WHERE
      string[] parts = whereClause.Split(' ');
      if (parts.Length != 3)
      {
        throw new ArgumentException("Cláusula WHERE inválida.");
      }

      string columnName = parts[0];
      string compareOperator = parts[1];
      string value = parts[2];

      // Verificar si existe un índice para la columna
      if (table.HasIndex && table.IndexColumn == columnName)
      {
        // Usar el índice para la búsqueda
        return UseIndexForSearch(table, columnName, compareOperator, value);
      }
      else
      {
        // Realizar una búsqueda secuencial
        return rows.Where(row => CompareValue(row[columnName], compareOperator, value)).ToList();
      }
    }

    private List<Dictionary<string, object>> UseIndexForSearch(Table table, string columnName, string compareOperator, string value)
    {
      // TODO: Implementar la búsqueda utilizando el índice
      // Esta es una implementación de ejemplo
      return new List<Dictionary<string, object>>();
    }

    private bool CompareValue(object rowValue, string compareOperator, string value)
    {
      // TODO: Implementar la comparación según el operador
      // Esta es una implementación de ejemplo
      return true;
    }

    private void ApplyOrderBy(List<Dictionary<string, object>> rows, string orderByColumn, bool isAscending)
    {
      // Implementar Quicksort para ordenar las filas
      QuickSort(rows, 0, rows.Count - 1, orderByColumn, isAscending);
    }

    private void QuickSort(List<Dictionary<string, object>> rows, int low, int high, string orderByColumn, bool isAscending)
    {
      if (low < high)
      {
        int partitionIndex = Partition(rows, low, high, orderByColumn, isAscending);

        QuickSort(rows, low, partitionIndex - 1, orderByColumn, isAscending);
        QuickSort(rows, partitionIndex + 1, high, orderByColumn, isAscending);
      }
    }

    private int Partition(List<Dictionary<string, object>> rows, int low, int high, string orderByColumn, bool isAscending)
    {
      var pivot = rows[high][orderByColumn];
      int i = low - 1;

      for (int j = low; j < high; j++)
      {
        if (CompareForSort(rows[j][orderByColumn], pivot, isAscending) <= 0)
        {
          i++;
          var temp = rows[i];
          rows[i] = rows[j];
          rows[j] = temp;
        }
      }

      var temp1 = rows[i + 1];
      rows[i + 1] = rows[high];
      rows[high] = temp1;

      return i + 1;
    }

    private int CompareForSort(object a, object b, bool isAscending)
    {
      // TODO: Implementar la comparación según el tipo de datos
      // Esta es una implementación de ejemplo
      return 0;
    }

    private List<Dictionary<string, object>> SelectColumns(List<Dictionary<string, object>> rows, List<string> columns)
    {
      return rows.Select(row => new Dictionary<string, object>(
          row.Where(kvp => columns.Contains(kvp.Key))
      )).ToList();
    }

  }




  /*
   * Esta clase maneja los paths del proyecto
   */
  internal class DataPath
  {
    private string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private string Database;
    private string? Table;

    public DataPath(string Name)
    {
      Database = Name;
      // Crear el directorio de la base de datos si no existe
      string databasePath = Path.Combine(AppDataPath, "DatabaseApp", Database);
      Directory.CreateDirectory(databasePath);
    }

    public void SaveTableAs(string name, object data)
    {
      Table = name;
      string filePath = GetTableFilePath();
      string jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(filePath, jsonData);
      Console.WriteLine($"Tabla '{name}' guardada exitosamente en {filePath}");
    }

    public T? LoadTable<T>(string name) where T : class
    {
      Table = name;
      string filePath = GetTableFilePath();
      if (!File.Exists(filePath))
      {
        Console.WriteLine($"La tabla '{name}' no existe en la base de datos '{Database}'.");
        return null;
      }

      string jsonData = File.ReadAllText(filePath);
      T? loadedData = JsonSerializer.Deserialize<T>(jsonData);
      Console.WriteLine($"Tabla '{name}' cargada exitosamente desde {filePath}");
      return loadedData;
    }

    private string GetTableFilePath()
    {
      if (string.IsNullOrEmpty(Table))
      {
        throw new InvalidOperationException("Nombre de tabla no especificado.");
      }
      return Path.Combine(AppDataPath, "DatabaseApp", Database, $"{Table}.json");
    }
  }

}
