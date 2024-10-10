using System;
using System.Collections.Generic;

namespace ApiInterface.Structures
{
  // Esta clase representa un nodo en un árbol B. Cada indice esta asociado a un objeto llamado object row. 
  // El InnerData es una lista de objetos Field, que representan los datos del nodo.
  // Osea, debe haber una Lista de Listas de objetos Field, donde cada lista de objetos Field representa una fila de datos.
  // El IsLeaf es un booleano que indica si el nodo es una hoja o no.
  // El Children es una lista de nodos que son los hijos del nodo.
  // El Keys es una lista de enteros que son las claves del nodo.
  // El T es el orden del árbol B.
  public class BTreeNode
  {
    public List<int> Keys { get; set; }
    public List<BTreeNode> Children { get; set; }
    public List<List<Field>> InnerData { get; set; }
    public bool IsLeaf { get; set; }

    public BTreeNode(bool isLeaf)
    {
      Keys = new List<int>();
      Children = new List<BTreeNode>();
      InnerData = new List<List<Field>>();
      IsLeaf = isLeaf;
    }
  }


  public class IndexBTree
  {
    public BTreeNode Root { get; set; }
    public int T { get; set; }

    public IndexBTree(int t)
    {
      Root = null;
      T = t;
    }

    public void Insert(int key, List<Field> value)
    {
      if (Root == null)
      {
        Root = new BTreeNode(true);
        Root.Keys.Add(key);
        Root.InnerData.Add(value);
      }
      else
      {
        InsertNonFull(Root, key, value);
      }
    }

    public void UpdateInnerData(int key, List<Field> value)
    {
      UpdateInnerData_aux(Root, key, value);
    }

    private void UpdateInnerData_aux(BTreeNode x, int key, List<Field> value)
    {
      int i = 0;
      while (i < x.Keys.Count && key > x.Keys[i])
      {
        i++;
      }
      if (i < x.Keys.Count && key == x.Keys[i])
      {
        x.InnerData[i] = value;
      } 
      else if (x.IsLeaf)
      {
        Console.WriteLine("Key not found");
      }
      else
      {
        UpdateInnerData_aux(x.Children[i], key, value);
      }
    } 



    private void InsertNonFull(BTreeNode x, int key, List<Field> value)
    {
      int i = x.Keys.Count - 1;
      if (x.IsLeaf)
      {
        x.Keys.Add(key);
        x.InnerData.Add(value);
      }
      else
      {
        while (i >= 0 && key < x.Keys[i])
        {
          i--;
        }
        i++;
        if (x.Children[i].Keys.Count == (2 * T) - 1)
        {
          SplitChild(x, i, x.Children[i]);
          if (key > x.Keys[i])
          {
            i++;
          }
        }
        InsertNonFull(x.Children[i], key, value);
      }
    }

    private void SplitChild(BTreeNode x, int i, BTreeNode y)
    {
      BTreeNode z = new BTreeNode(y.IsLeaf);
      x.Children.Insert(i + 1, z);
      x.Keys.Insert(i, y.Keys[T - 1]);
      z.Keys = y.Keys.GetRange(T, T - 1);
      y.Keys = y.Keys.GetRange(0, T - 1);
      z.InnerData = y.InnerData.GetRange(T, T);
      y.InnerData = y.InnerData.GetRange(0, T); 
      z.Children = y.Children.GetRange(T, T);
      y.Children = y.Children.GetRange(0, T);
    }

    public List<Field> Search(int key)
    {
      return Search_aux(Root, key);
    } 

    private List<Field> Search_aux(BTreeNode x, int key)
    {
      int i = 0;
      while (i < x.Keys.Count && key > x.Keys[i])
      {
        i++;
      }
      if (i < x.Keys.Count && key == x.Keys[i])
      {
        return x.InnerData[i];
      }
      else if (x.IsLeaf)
      {
        return new List<Field>();
      }
      else
      {
        return Search_aux(x.Children[i], key);
      }
    }

    public void Delete(int key)
    {
      Delete(Root, key);
    }

    private void Delete(BTreeNode x, int key)
    {
      int i = 0;
      while (i < x.Keys.Count && key > x.Keys[i])
      {
        i++;
      }
      if (i < x.Keys.Count && key == x.Keys[i])
      {
        DeleteKey(x, i);
      }
      else if (x.IsLeaf)
      {
        Console.WriteLine("Key not found");
      }
      else
      {
        Delete(x.Children[i], key);
      }
    } 

    private void DeleteKey(BTreeNode x, int i)
    {
      if (x.IsLeaf)
      {
        x.Keys.RemoveAt(i);
        x.InnerData.RemoveAt(i);
      }
      else
      {
        DeleteFromNonLeaf(x, i);
      }
    } 

    private void DeleteFromNonLeaf(BTreeNode x, int i)
    {
      int key = x.Keys[i];  
      if (x.Children[i].Keys.Count >= T)
      {
        int pred = GetPredecessor(x, i);
        x.Keys[i] = pred;
        x.InnerData[i] = Search(pred);
        Delete(x.Children[i], pred);
      }
      else if (x.Children[i + 1].Keys.Count >= T)
      {
        int succ = GetSuccessor(x, i);
        x.Keys[i] = succ;
        x.InnerData[i] = Search(succ);
        Delete(x.Children[i + 1], succ);
      }
      else
      { 
        Merge(x, i);
        Delete(x.Children[i], key);
      }
    } 

    private int GetPredecessor(BTreeNode x, int i)
    {
      BTreeNode cur = x.Children[i];
      while (!cur.IsLeaf)
      {
        cur = cur.Children[cur.Keys.Count];
      }
      return cur.Keys[cur.Keys.Count - 1];
    }

    private int GetSuccessor(BTreeNode x, int i)
    {
      BTreeNode cur = x.Children[i + 1];  
      while (!cur.IsLeaf)
      {
        cur = cur.Children[0];
      }
      return cur.Keys[0];
    }

    private void Merge(BTreeNode x, int i)
    { 
      BTreeNode y = x.Children[i];
      BTreeNode z = x.Children[i + 1];
      y.Keys.Add(x.Keys[i]);
      y.InnerData.Add(x.InnerData[i]);
      y.Keys.AddRange(z.Keys);
      y.InnerData.AddRange(z.InnerData);
      y.Children.AddRange(z.Children);
      x.Keys.RemoveAt(i);
      x.InnerData.RemoveAt(i);
      x.Children.RemoveAt(i + 1); 
    }

    public override string ToString()
    {
      return ToString_aux(Root);
    }

    private string ToString_aux(BTreeNode x)
    {
      if (x == null)
      {
        return "";
      }
      string result = "";
      foreach (var key in x.Keys)
      {
        result += key + ": " + string.Join(", ", x.InnerData[x.Keys.IndexOf(key)]) + "\n";
      }
      foreach (var child in x.Children)
      {
        result += ToString_aux(child);
      }
      return result;
    }
    
  }
}

  
