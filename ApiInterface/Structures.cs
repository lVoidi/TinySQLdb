/*
 * TODO: 
 *    (1) Hacer que CreateIndex reciba como argumento la columna de la tabla a la que se 
 *    va a aplicar el indice
 *    (2) Completar los arboles, con una funcion de busqueda incluida
 */
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
    private IndexBinaryTree? Tree;
    public string Name;
    public bool HasIndex = false;
    public string IndexColumn = "";
    public TableIndex Index = TableIndex.None;
    public List<Field> TableFields;

    public Table(string name, List<Field> tableFields)
    {
      Name = name;
      TableFields = tableFields;
    }

    public void Insert(string row)
    {
    }

    public void Delete(string row)
    {
    }

    public override string ToString()
    {
      return "";
    }

    // TODO: (1)
    public void CreateIndex(TableIndex index)
    {
      HasIndex = true;
      if (index == TableIndex.BSTree)
      {
        Tree = new IndexBinaryTree();
      }
      else
      {
        Tree = new IndexBSTree();
      }
    }
  }

  internal class IndexTreeNode
  {
    public string? Data;
    public int? Index;
  }

  // TODO: (1) (2)
  internal class IndexBinaryTree
  {
    public IndexBinaryTree() { }
    public virtual void Insert() { }
    public virtual void Delete() { }
  }
  // TODO: (1) (2)
  internal class IndexBSTree : IndexBinaryTree
  {
    public IndexBSTree() { }
    public override void Insert() { }
    public override void Delete() { }
  }

}
