using System;
using System.Collections.Generic;
using ApiInterface.Models;

namespace ApiInterface.Structures
{
  internal class IndexBTreeNode
  {
    public List<int> Keys { get; set; }
    public List<IndexBTreeNode> Children { get; set; }
    public bool IsLeaf { get; set; }
    public List<Field> InnerData { get; set; }

    public IndexBTreeNode(bool isLeaf)
    {
      Keys = new List<int>();
      Children = new List<IndexBTreeNode>();
      IsLeaf = isLeaf;
      InnerData = new List<Field>();
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

    public void Insert(int key, List<Field> fields)
    {
      IndexBTreeNode r = root;

      if (r.Keys.Count == (2 * t - 1))
      {
        IndexBTreeNode s = new IndexBTreeNode(false);
        root = s;
        s.Children.Add(r);
        SplitChild(s, 0, r);
        InsertNonFull(s, key, fields);
      }
      else
      {
        InsertNonFull(r, key, fields);
      }
    }

    private void InsertNonFull(IndexBTreeNode x, int key, List<Field> fields)
    {
      int i = x.Keys.Count - 1;

      if (x.IsLeaf)
      {
        x.Keys.Add(0);
        x.InnerData.Add(null);
        while (i >= 0 && key < x.Keys[i])
        {
          x.Keys[i + 1] = x.Keys[i];
          x.InnerData[i + 1] = x.InnerData[i];
          i--;
        }
        x.Keys[i + 1] = key;
        x.InnerData[i + 1] = fields[0]; // Asumimos que solo se inserta un Field por clave
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
        InsertNonFull(x.Children[i], key, fields);
      }
    }

    private void SplitChild(IndexBTreeNode x, int i, IndexBTreeNode y)
    {
      IndexBTreeNode z = new IndexBTreeNode(y.IsLeaf);
      x.Children.Insert(i + 1, z);
      x.Keys.Insert(i, y.Keys[t - 1]);
      x.InnerData.Insert(i, y.InnerData[t - 1]);

      for (int j = 0; j < t - 1; j++)
      {
        z.Keys.Add(y.Keys[j + t]);
        z.InnerData.Add(y.InnerData[j + t]);
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
      y.InnerData.RemoveRange(t - 1, t);
    }

    public Field Find(int key)
    {
      return Search(root, key);
    }

    private Field Search(IndexBTreeNode x, int key)
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

      if (x.IsLeaf)
      {
        return null;
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
}