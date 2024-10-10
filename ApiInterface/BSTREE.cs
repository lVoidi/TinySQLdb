using System.Text.RegularExpressions;
using ApiInterface.Models;
using System;
using System.Collections.Generic;

namespace ApiInterface.Structures
{
internal class IndexBSTreeNode
  {
    public int Key { get; set; }
    public List<Field> InnerData { get; set; }
    public IndexBSTreeNode Left { get; set; }
    public IndexBSTreeNode Right { get; set; }
    public int Height { get; set; }

    public IndexBSTreeNode(int key, List<Field> innerData)
    {
      Key = key;
      InnerData = innerData;
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

    public void Insert(int key, List<Field> innerData)
    {
      root = InsertRec(root, key, innerData);
    }

    private IndexBSTreeNode InsertRec(IndexBSTreeNode node, int key, List<Field> innerData)
    {
      if (node == null)
        return new IndexBSTreeNode(key, innerData);

      if (key < node.Key)
        node.Left = InsertRec(node.Left, key, innerData);
      else if (key > node.Key)
        node.Right = InsertRec(node.Right, key, innerData);
      else
      {
        // Si la clave ya existe, actualizamos los datos internos
        node.InnerData = innerData;
        return node;
      }

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

    public List<Field> Find(int key)
    {
      return FindRec(root, key);
    }

    private List<Field> FindRec(IndexBSTreeNode root, int key)
    {
      if (root == null)
        return null;
      if (root.Key == key)
        return root.InnerData;
      if (key < root.Key)
        return FindRec(root.Left, key);
      return FindRec(root.Right, key);
    }
  }
}