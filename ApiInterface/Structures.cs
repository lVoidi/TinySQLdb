/*
 * TODO: 
 *    (1) Hacer que CreateIndex reciba como argumento la columna de la tabla a la que se 
 *    va a aplicar el indice
 *    (2) Completar los arboles, con una funcion de busqueda incluida
 */
using ApiInterface.Models;
namespace ApiInterface.Structures
{
  internal class Table
  {
    public int Columns;
    public TableIndex Index = TableIndex.None;
    private IndexBinaryTree? Tree;

    public Table(int columns)
    {
      Columns = columns;
    }

    public void Insert(string row)
    {
    }

    public void Delete(string row)
    {
    }
    
    // TODO: (1)
    public void CreateIndex(TableIndex index)
    {
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
