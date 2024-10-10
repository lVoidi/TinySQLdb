// TODO: Mover los arboles a archivos individuales
// TODO: Hacer que la funcion find individual de cada arbol devuelva una List<fields>, lo cual 
// corresponde a una fila 
// TODO: Hacer que la funcion Table.Delete elimine la fila que se pida
using System.Text.RegularExpressions;
using ApiInterface.Models;
using System;

namespace ApiInterface.Structures
{
  public class Field
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

  public class Table
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
        BTree.Insert(key, field);
      }
      else if (Index == TableIndex.BSTree)
      {
        BSTree.Insert(key, field);
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
        foreach (List<Field> row in TableFields){
          foreach (Field field in row){
            if (field.Value == key.ToString()){
              row.Remove(field);
            }
          }
        }
      }
      else if (Index == TableIndex.BSTree)
      {
        BSTree.Delete(key);
        foreach (List<Field> row in TableFields){
          foreach (Field field in row){
            if (field.Value == key.ToString()){
              row.Remove(field);
            }
          }
        }
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
        BTree = new IndexBTree(3);
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
        return BSTree.Find(id);
      }
      else if (Index == TableIndex.BTree)
      {
        return BTree.Search(id);
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
          Match match = Regex.Match(column.Name, patternWhere);
          if (match.Success)
          {
            whereColumn = column.Name;
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
}
