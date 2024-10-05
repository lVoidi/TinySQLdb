/*
 * TODO:
 *    (1) Agregar los paths de metadatos
 *    (2) Terminar la funcion CreateTableAs
 *    (3) Hacer funciones capaces de serializar y deserializar tablas
 *    (4) Agregar todas las otras funciones de SQL
 */
using ApiInterface.Structures;
using System.Text.Json;
using System.IO;
using System.Data;
namespace ApiInterface.Store
{
  /*
   * Esta clase guarda los datos en memoria, donde Table es la tabla. 
   */
  internal class Data
  {
    private string? FileData;
    private string? Name;
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

      // Generar la sentencia SQL CREATE TABLE (opcional, para referencia)
      string createTableSQL = $"CREATE TABLE {name} (\n";
      createTableSQL += string.Join(",\n", tableFields.Select(f => 
          $"  {f.Name} {f.Type}{(f.Type == "VARCHAR" ? $"({f.Size})" : "")}"));
      createTableSQL += "\n);";

      // Guardar la definición de la tabla si es necesario
      if (Path != null)
      {
        Path.SaveTableAs(name, createTableSQL);
      }
    }

    // TODO: (3)
    // Serializa los datos de Table
    public void SerializeData()
    {
        if (Tables.Count == 0 || string.IsNullOrEmpty(Name))
        {
            throw new InvalidOperationException("No hay tablas para serializar o no se ha definido un nombre.");
        }

        string jsonString = JsonSerializer.Serialize(Tables);
        string fileName = $"{Name}.json";
        File.WriteAllText(fileName, jsonString);
    }
  
    // TODO: (3)
    // Deserializa los datos de una Tabla 
    public void DeserializeData()
    {
        if (string.IsNullOrEmpty(Name))
        {
            throw new InvalidOperationException("El nombre de la base de datos no está definido.");
        }

        string fileName = $"{Name}.json";
        if (File.Exists(fileName))
        {
            string jsonString = File.ReadAllText(fileName);
            Tables = JsonSerializer.Deserialize<List<Table>>(jsonString) ?? new List<Table>();
        }
        else
        {
            throw new FileNotFoundException($"No se encontró el archivo {fileName}");
        }
    }

    // TODO: (4)
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
        // Aquí iría la lógica para verificar si la tabla existe
        return true;
    }

    private bool IsTableEmpty(string tableName)
    {
        // Aquí iría la lógica para verificar si la tabla está vacía
        return true;
    }

  }

  public class Table
  {
    public string Name { get; set; }
    public List<Field> Fields { get; set; }

    public Table(string name, List<Field> fields)
    {
        Name = name;
        Fields = fields;
    }
  }

  public class Field
  {
    public string Name { get; }
    public string Type { get; }
    public int? Size { get; }

    public Field(string name, string type, int? size = null)
    {
      Name = name;
      Type = type;
      Size = size;
    }
  }

  /*
   * Esta clase maneja los paths del proyecto
   */
  internal class DataPath
  {
    // TODO: (1)
    private string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private string Database;
    private string? Table;

    public DataPath(string Name)
    {
      Database = Name;
    }

    public void SaveTableAs(string name, string data){}
    public void LoadTable(string name){}
  }

}