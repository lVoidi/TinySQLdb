/*
 * TODO:
 *    (1) Agregar los paths de metadatos
 *    (2) Terminar la funcion CreateTableAs
 *    (3) Hacer funciones capaces de serializar y deserializar tablas
 *    (4) Agregar todas las otras funciones de SQL
 */
using ApiInterface.Structures;
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
    private Table? Table;
    
    // TODO: (2)
    // fields es un string tipo: "ID Integer,Name Varchar(20)" 
    public void CreateTableAs(string name, string fields)
    {
      Name = name;
      string[] fieldDefinitions = fields.Split(',');
      List<string> tableFields = new List<string>();
      
      foreach (string fieldDef in fieldDefinitions)
      {
        string[] parts = fieldDef.Trim().Split(' ');
        if (parts.Length >= 2)
        {
          string fieldName = parts[0];
          string fieldType = parts[1];
          int? fieldSize = null;
          
          if (fieldType.Contains("("))
          {
            int startIndex = fieldType.IndexOf("(");
            int endIndex = fieldType.IndexOf(")");
            if (int.TryParse(fieldType.Substring(startIndex + 1, endIndex - startIndex - 1), out int size))
            {
              fieldSize = size;
            }
            fieldType = fieldType.Substring(0, startIndex);
          }
          
          tableFields.Add($"{fieldName},{fieldType},{fieldSize}");
        }
      }
      
      Table = new Table(tableFields.Count);

      // Aquí podrías agregar la lógica para guardar la tabla en el Path si es necesario
      if (Path != null)
      {
        Path.SaveTableAs(name, fields); // Asumiendo que quieres guardar la definición de la tabla
      }
    }

    // TODO: (3)
    // Serializa los datos de Table
    public void SerializeData()
    {
    }
  
    // TODO: (3)
    // Deserializa los datos de una Tabla 
    public void DeserializeData()
    {
    }

    // TODO: (4)
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
