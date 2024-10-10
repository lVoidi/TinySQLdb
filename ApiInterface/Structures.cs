using ApiInterface.Models;
namespace ApiInterface.Structures
{
  internal class Field
  {
    public string Name;
    public string Type;
    public int? Size;

    public Field(string name, string type, int? size)
    {
      Name = name;
      Type = type;
      Size = size;
    }
  }

  internal class Table
  {
    // Estos pasan a ser instancias de sus respectivas clases cuando 
    // se llama CreateIndex
    private IndexBTree? BTree = null;
    private IndexBSTree? BSTree = null;

    // Literalmente el nombre del archivo 
    public string Name;

    // Se pone como verdadera cuando se crea el indice
    public bool HasIndex = false;

    // Enum que dice que tipo de indice tiene la tabla 
    public TableIndex Index = TableIndex.None;

    // Cada tabla tiene siempre su lista de Fields con sus respectivos 
    // datos. Para cada insert, se agrega un field.
    public List<Field> TableFields;

    public Table(string name, List<Field> tableFields)
    {
      Name = name;
      TableFields = tableFields;
    }

    // Este insert debe depender del tipo de indice
    // 1. Si TableIndex == None:
    //    se recorre tableFields
    // 2. Si TableIndex == BTree:
    //    se recorre el BTree 
    // 3. Si no: 
    //    se recorre BSTree
    public void Insert()
    {
    }

    public void Delete()
    {
    }

    // Recorrer todos los fields de List<Field> e imprimirlos con 
    // Console.WriteLine()
    public override string ToString()
    {
      return "";
    }

    public void CreateIndex(TableIndex index)
    {
      if (index == TableIndex.None)
      {

      }
      else if (index == TableIndex.BTree)
      {

      }
      else
      {

      }
    }
  }

  internal class IndexBTreeNode
  {
  }

  internal class IndexBSTreeNode
  {
  }

  internal class IndexBTree
  {
    public IndexBTree() { }
    public void Insert() { }
    public void Delete() { }
  }

  // Esto debe ser un arbol AVL para que se balancee
  internal class IndexBSTree
  {
    public IndexBSTree() { }
    public void Insert() { }
    public void Delete() { }
  }

}
