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
