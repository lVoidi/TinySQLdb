// TODO: Mover los arboles a archivos individuales
// TODO: Hacer que la funcion find individual de cada arbol devuelva una List<fields>, lo cual 
// corresponde a una fila 
// TODO: Hacer que la funcion Table.Delete elimine la fila que se pida
using System.Text.RegularExpressions;
using ApiInterface.Models;
using System;

namespace ApiInterface.Structures
{
  internal class Field
  {
    public string Name;
    public string Type;
    public string Value;
    public int? Size;

    public Field(string name, string type, int? size, string val)
    {
      Name = name;
      Type = type;
      Size = size;
      Value = val;
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

    public string? columnWithIndex = null;

    // Enum que dice que tipo de indice tiene la tabla 
    public TableIndex Index = TableIndex.None;

    // Headers de la tabla 
    // Ejemplo: nombre, apellido, nacimiento
    public LinkedList<Field> Headers;

    // Cada tabla tiene siempre su lista de Fields con sus respectivos 
    // datos. Para cada insert, se agrega un field.
    // Ejemplo: carlos, fernandez, 2000-01-01 01:02:00
    public List<List<Field>> TableFields;

    public Table(string name, List<List<Field>> tableFields, LinkedList<Field> headers)
    {
      Name = name;
      TableFields = tableFields;
      Headers = headers;
    }

    public void Insert(int key, List<Field> field)
    {
      if (BTree == null || BSTree == null)
        return;
      if (Index == TableIndex.BTree)
      {
        BTree.Insert(key);
      }
      else if (Index == TableIndex.BSTree)
      {
        BSTree.Insert(key);
      }
      TableFields.Add(field);
    }
    
    // TODO: Hacer que elimine el field correspondiente en TableFields
    public void Delete(int key)
    {
      if (BTree == null || BSTree == null)
        return;
      if (!HasIndex)
      {
        Console.WriteLine("No se puede eliminar sin un índice.");
      }
      else if (Index == TableIndex.BTree)
      {
        BTree.Delete(key);
        // Aquí deberías también eliminar el Field correspondiente de TableFields
      }
      else if (Index == TableIndex.BSTree)
      {
        BSTree.Delete(key);
        // Aquí deberías también eliminar el Field correspondiente de TableFields
      }
    }

    public override string ToString()
    {
      string result = $"Tabla: {Name}\n";
      foreach (List<Field> row in TableFields)
      {
        foreach (Field field in row)
        {
          result += $"{field.Value} ";
        }
        result += "\n";
      }
      return result;
    }

    public void CreateIndex(TableIndex index, string columnName)
    {
      if (HasIndex)
      {
        Console.WriteLine("Ya existe un índice para esta tabla.");
        return;
      }

      Headers.AddFirst(new Field("index", "index", 100, ""));

      if (index == TableIndex.None)
      {
        return;
      }
      else if (index == TableIndex.BTree)
      {
        BTree = new IndexBTree(3); // Asumimos un grado mínimo de 3 para el árbol B
        Index = TableIndex.BTree;
      }
      else
      {
        BSTree = new IndexBSTree();
        Index = TableIndex.BSTree;
      }
      HasIndex = true;
      columnWithIndex = columnName;
    }

    public List<Field>? FindById(int id)
    {
      if (Index == TableIndex.None)
      {
        return TableFields[id];
      }
      else if (Index == TableIndex.BSTree)
      {

      }
      else if (Index == TableIndex.BTree)
      {

      }
      return null;
    }

    public List<List<Field>> SelectWhere(string patternWhere, string? patternLike = null)
    {
      string whereColumn = "";
      List<List<Field>>? result = new();
      foreach (List<Field> row in TableFields)
      {
        foreach (Field column in row)
        {
          if (whereColumn == "")
          {
            Match match = Regex.Match(column.Name, patternWhere);
            if (match.Success)
            {
              whereColumn = column.Name;
            }
          }
          else if (whereColumn == column.Name)
          {
            if (patternLike != null)
            {
              Match matchLike = Regex.Match(column.Name, patternLike);
              if (!matchLike.Success)
              {
                continue;
              }
            }
            result.Add(row);
          }
        }
      }
      return result;
    }
  }

  internal class IndexBTreeNode
  {
    public List<int> Keys { get; set; }
    public List<IndexBTreeNode> Children { get; set; }
    public bool IsLeaf { get; set; }

    public IndexBTreeNode(bool isLeaf)
    {
      Keys = new List<int>();
      Children = new List<IndexBTreeNode>();
      IsLeaf = isLeaf;
    }
  }

  internal class IndexBTree
  {
    private IndexBTreeNode root;
    private int t; // Grado mínimo del árbol B

    public IndexBTree(int t)
    {
      this.root = new IndexBTreeNode(true);
      this.t = t;
    }

    public void Insert(int key)
    {
      IndexBTreeNode r = root;

      if (r.Keys.Count == (2 * t - 1))
      {
        IndexBTreeNode s = new IndexBTreeNode(false);
        root = s;
        s.Children.Add(r);
        SplitChild(s, 0, r);
        InsertNonFull(s, key);
      }
      else
      {
        InsertNonFull(r, key);
      }
    }

    private void InsertNonFull(IndexBTreeNode x, int key)
    {
      int i = x.Keys.Count - 1;

      if (x.IsLeaf)
      {
        x.Keys.Add(0);
        while (i >= 0 && key < x.Keys[i])
        {
          x.Keys[i + 1] = x.Keys[i];
          i--;
        }
        x.Keys[i + 1] = key;
      }
      else
      {
        while (i >= 0 && key < x.Keys[i])
        {
          i--;
        }
        i++;

        if (x.Children[i].Keys.Count == (2 * t - 1))
        {
          SplitChild(x, i, x.Children[i]);
          if (key > x.Keys[i])
          {
            i++;
          }
        }
        InsertNonFull(x.Children[i], key);
      }
    }

    private void SplitChild(IndexBTreeNode x, int i, IndexBTreeNode y)
    {
      IndexBTreeNode z = new IndexBTreeNode(y.IsLeaf);
      x.Children.Insert(i + 1, z);
      x.Keys.Insert(i, y.Keys[t - 1]);

      for (int j = 0; j < t - 1; j++)
      {
        z.Keys.Add(y.Keys[j + t]);
      }

      if (!y.IsLeaf)
      {
        for (int j = 0; j < t; j++)
        {
          z.Children.Add(y.Children[j + t]);
        }
        y.Children.RemoveRange(t, t);
      }

      y.Keys.RemoveRange(t - 1, t);
    }

    public bool Find(int key)
    {
      return Search(root, key) != null;
    }

    private IndexBTreeNode Search(IndexBTreeNode x, int key)
    {
      int i = 0;
      while (i < x.Keys.Count && key > x.Keys[i])
      {
        i++;
      }

      if (i < x.Keys.Count && key == x.Keys[i])
      {
        return x;
      }

      if (x.IsLeaf)
      {
        return new IndexBTreeNode(true);
      }

      return Search(x.Children[i], key);
    }

    public void Delete(int key)
    {
      if (root == null)
        return;

      DeleteInternal(root, key);

      if (root.Keys.Count == 0 && !root.IsLeaf)
      {
        root = root.Children[0];
      }
    }

    private void DeleteInternal(IndexBTreeNode x, int key)
    {
      int idx = FindKeyIndex(x, key);

      if (idx < x.Keys.Count && x.Keys[idx] == key)
      {
        if (x.IsLeaf)
          DeleteFromLeaf(x, idx);
        else
          DeleteFromNonLeaf(x, idx);
      }
      else
      {
        if (x.IsLeaf)
        {
          Console.WriteLine($"La clave {key} no existe en el árbol");
          return;
        }

        bool flag = (idx == x.Keys.Count);

        if (x.Children[idx].Keys.Count < t)
          Fill(x, idx);

        if (flag && idx > x.Keys.Count)
          DeleteInternal(x.Children[idx - 1], key);
        else
          DeleteInternal(x.Children[idx], key);
      }
    }

    private int FindKeyIndex(IndexBTreeNode x, int key)
    {
      int idx = 0;
      while (idx < x.Keys.Count && x.Keys[idx] < key)
        ++idx;
      return idx;
    }

    private void DeleteFromLeaf(IndexBTreeNode x, int idx)
    {
      for (int i = idx + 1; i < x.Keys.Count; ++i)
        x.Keys[i - 1] = x.Keys[i];

      x.Keys.RemoveAt(x.Keys.Count - 1);
    }

    private void DeleteFromNonLeaf(IndexBTreeNode x, int idx)
    {
      int k = x.Keys[idx];

      if (x.Children[idx].Keys.Count >= t)
      {
        int pred = GetPred(x, idx);
        x.Keys[idx] = pred;
        DeleteInternal(x.Children[idx], pred);
      }
      else if (x.Children[idx + 1].Keys.Count >= t)
      {
        int succ = GetSucc(x, idx);
        x.Keys[idx] = succ;
        DeleteInternal(x.Children[idx + 1], succ);
      }
      else
      {
        Merge(x, idx);
        DeleteInternal(x.Children[idx], k);
      }
    }

    private int GetPred(IndexBTreeNode x, int idx)
    {
      IndexBTreeNode cur = x.Children[idx];
      while (!cur.IsLeaf)
        cur = cur.Children[cur.Children.Count - 1];

      return cur.Keys[cur.Keys.Count - 1];
    }

    private int GetSucc(IndexBTreeNode x, int idx)
    {
      IndexBTreeNode cur = x.Children[idx + 1];
      while (!cur.IsLeaf)
        cur = cur.Children[0];

      return cur.Keys[0];
    }

    private void Fill(IndexBTreeNode x, int idx)
    {
      if (idx != 0 && x.Children[idx - 1].Keys.Count >= t)
        BorrowFromPrev(x, idx);
      else if (idx != x.Keys.Count && x.Children[idx + 1].Keys.Count >= t)
        BorrowFromNext(x, idx);
      else
      {
        if (idx != x.Keys.Count)
          Merge(x, idx);
        else
          Merge(x, idx - 1);
      }
    }

    private void BorrowFromPrev(IndexBTreeNode x, int idx)
    {
      IndexBTreeNode child = x.Children[idx];
      IndexBTreeNode sibling = x.Children[idx - 1];

      for (int i = child.Keys.Count - 1; i >= 0; --i)
        child.Keys[i + 1] = child.Keys[i];

      if (!child.IsLeaf)
      {
        for (int i = child.Children.Count - 1; i >= 0; --i)
          child.Children[i + 1] = child.Children[i];
      }

      child.Keys[0] = x.Keys[idx - 1];

      if (!child.IsLeaf)
        child.Children[0] = sibling.Children[sibling.Children.Count - 1];

      x.Keys[idx - 1] = sibling.Keys[sibling.Keys.Count - 1];

      child.Keys.Insert(0, x.Keys[idx - 1]);
      sibling.Keys.RemoveAt(sibling.Keys.Count - 1);

      if (!child.IsLeaf)
      {
        child.Children.Insert(0, sibling.Children[sibling.Children.Count - 1]);
        sibling.Children.RemoveAt(sibling.Children.Count - 1);
      }
    }

    private void BorrowFromNext(IndexBTreeNode x, int idx)
    {
      IndexBTreeNode child = x.Children[idx];
      IndexBTreeNode sibling = x.Children[idx + 1];

      child.Keys.Add(x.Keys[idx]);

      if (!child.IsLeaf)
        child.Children.Add(sibling.Children[0]);

      x.Keys[idx] = sibling.Keys[0];

      for (int i = 1; i < sibling.Keys.Count; ++i)
        sibling.Keys[i - 1] = sibling.Keys[i];

      if (!sibling.IsLeaf)
      {
        for (int i = 1; i < sibling.Children.Count; ++i)
          sibling.Children[i - 1] = sibling.Children[i];
      }

      sibling.Keys.RemoveAt(sibling.Keys.Count - 1);

      if (!sibling.IsLeaf)
        sibling.Children.RemoveAt(sibling.Children.Count - 1);
    }

    private void Merge(IndexBTreeNode x, int idx)
    {
      IndexBTreeNode child = x.Children[idx];
      IndexBTreeNode sibling = x.Children[idx + 1];

      child.Keys.Add(x.Keys[idx]);

      for (int i = 0; i < sibling.Keys.Count; ++i)
        child.Keys.Add(sibling.Keys[i]);

      if (!child.IsLeaf)
      {
        for (int i = 0; i <= sibling.Children.Count; ++i)
          child.Children.Add(sibling.Children[i]);
      }

      for (int i = idx + 1; i < x.Keys.Count; ++i)
        x.Keys[i - 1] = x.Keys[i];

      for (int i = idx + 2; i < x.Children.Count; ++i)
        x.Children[i - 1] = x.Children[i];

      x.Keys.RemoveAt(x.Keys.Count - 1);
      x.Children.RemoveAt(x.Children.Count - 1);
    }
  }

  internal class IndexBSTreeNode
  {
    public int Key { get; set; }
    public IndexBSTreeNode Left { get; set; }
    public IndexBSTreeNode Right { get; set; }
    public int Height { get; set; }

    public IndexBSTreeNode(int key)
    {
      Key = key;
      Height = 1;
    }
  }

  internal class IndexBSTree
  {
    private IndexBSTreeNode root;

    public IndexBSTree()
    {
      root = null;
    }

    private int Height(IndexBSTreeNode node)
    {
      return node == null ? 0 : node.Height;
    }

    private int BalanceFactor(IndexBSTreeNode node)
    {
      return node == null ? 0 : Height(node.Left) - Height(node.Right);
    }

    private void UpdateHeight(IndexBSTreeNode node)
    {
      node.Height = 1 + Math.Max(Height(node.Left), Height(node.Right));
    }

    private IndexBSTreeNode RotateRight(IndexBSTreeNode y)
    {
      IndexBSTreeNode x = y.Left;
      IndexBSTreeNode T2 = x.Right;

      x.Right = y;
      y.Left = T2;

      UpdateHeight(y);
      UpdateHeight(x);

      return x;
    }

    private IndexBSTreeNode RotateLeft(IndexBSTreeNode x)
    {
      IndexBSTreeNode y = x.Right;
      IndexBSTreeNode T2 = y.Left;

      y.Left = x;
      x.Right = T2;

      UpdateHeight(x);
      UpdateHeight(y);

      return y;
    }

    public void Insert(int key)
    {
      root = InsertRec(root, key);
    }

    private IndexBSTreeNode InsertRec(IndexBSTreeNode node, int key)
    {
      if (node == null)
        return new IndexBSTreeNode(key);

      if (key < node.Key)
        node.Left = InsertRec(node.Left, key);
      else if (key > node.Key)
        node.Right = InsertRec(node.Right, key);
      else
        return node; // Duplicate keys not allowed

      UpdateHeight(node);

      int balance = BalanceFactor(node);

      // Left Left Case
      if (balance > 1 && key < node.Left.Key)
        return RotateRight(node);

      // Right Right Case
      if (balance < -1 && key > node.Right.Key)
        return RotateLeft(node);

      // Left Right Case
      if (balance > 1 && key > node.Left.Key)
      {
        node.Left = RotateLeft(node.Left);
        return RotateRight(node);
      }

      // Right Left Case
      if (balance < -1 && key < node.Right.Key)
      {
        node.Right = RotateRight(node.Right);
        return RotateLeft(node);
      }

      return node;
    }

    public void Delete(int key)
    {
      root = DeleteRec(root, key);
    }

    private IndexBSTreeNode DeleteRec(IndexBSTreeNode root, int key)
    {
      if (root == null)
        return null;

      if (key < root.Key)
        root.Left = DeleteRec(root.Left, key);
      else if (key > root.Key)
        root.Right = DeleteRec(root.Right, key);
      else
      {
        if (root.Left == null || root.Right == null)
        {
          IndexBSTreeNode? temp = null;
          if (temp == root.Left)
            temp = root.Right;
          else
            temp = root.Left;

          if (temp is null)
          {
            temp = root;
            root = null;
          }
          else
            root = temp;
        }
        else
        {
          IndexBSTreeNode temp = MinValueNode(root.Right);
          root.Key = temp.Key;
          root.Right = DeleteRec(root.Right, temp.Key);
        }
      }

      if (root == null)
        return root;

      UpdateHeight(root);

      int balance = BalanceFactor(root);

      // Left Left Case
      if (balance > 1 && BalanceFactor(root.Left) >= 0)
        return RotateRight(root);

      // Left Right Case
      if (balance > 1 && BalanceFactor(root.Left) < 0)
      {
        root.Left = RotateLeft(root.Left);
        return RotateRight(root);
      }

      // Right Right Case
      if (balance < -1 && BalanceFactor(root.Right) <= 0)
        return RotateLeft(root);

      // Right Left Case
      if (balance < -1 && BalanceFactor(root.Right) > 0)
      {
        root.Right = RotateRight(root.Right);
        return RotateLeft(root);
      }

      return root;
    }

    private IndexBSTreeNode MinValueNode(IndexBSTreeNode node)
    {
      IndexBSTreeNode current = node;
      while (current.Left != null)
        current = current.Left;
      return current;
    }

    public bool Find(int key)
    {
      return FindRec(root, key);
    }

    private bool FindRec(IndexBSTreeNode root, int key)
    {
      if (root == null)
        return false;
      if (root.Key == key)
        return true;
      if (key < root.Key)
        return FindRec(root.Left, key);
      return FindRec(root.Right, key);
    }
  }
}
