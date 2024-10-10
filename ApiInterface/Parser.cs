/*
 * TODO: 
 *    (3) Completar la funcion que haga el parse de cada comando individual
 */
using System.Text.RegularExpressions;
using System.Diagnostics;
using ApiInterface.Models;
using ApiInterface.Store;
using ApiInterface.Structures;
namespace ApiInterface.Parser
{
  public class SQLQueryProcessor
  {
    public static List<string> Sentences = new();
    private static Data? data;

    public static OperationStatus Execute(string script)
    {
      data = new();
      Sentences = AddSentences(script);
      OperationStatus result = OperationStatus.Success;
      Console.WriteLine(script);
      if (Sentences.Count == 0)
      {
        return OperationStatus.Error;
      }

      foreach (string Sentence in Sentences)
      {
        Console.WriteLine($"PARSE: {Sentence}");
        if (!HasCorrectParenthesis(Sentence))
        {
          Console.WriteLine("Error: Parentesis incorrectos");
          return OperationStatus.Error;
        }
        Stopwatch stopwatch = new();
        stopwatch.Start();
        try
        {
          result = Parse(Sentence);
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
          return OperationStatus.Error;
        }
        stopwatch.Stop();
        Console.WriteLine($"TIME: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"RESULT: {result}");
        if (result == OperationStatus.Warning || result == OperationStatus.Error)
        {
          return result;
        }
      }

      try 
      {
        data.Save();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        return OperationStatus.Error;
      } 
      return result;
    }

    /*
     * Este es el que separa las lineas de codigo por puntos y comas
     * Elimina Espacios en blanco extra, tabs y newlines
     */
    public static List<string> AddSentences(string script)
    {
      // Este patron separa por ;, ignorando los espacios en blanco alrededor
      string pattern = @"\s*;\s*";
      string[] sentences = Regex.Split(script.Trim(), pattern);
      List<string> NewSentences = new();
      foreach (string sentence in sentences)
      {
        if (!string.IsNullOrWhiteSpace(sentence))
        {
          NewSentences.Add(RemoveExtraWhitespaces(sentence.Trim()));
        }
      }
      return NewSentences;
    }

    private static string RemoveExtraWhitespaces(string sentence)
    {
      return Regex.Replace(sentence, @"\s+", " ");
    }

    /*
     * Esta funcion se encarga de chequear los parentesis 
     */
    public static bool HasCorrectParenthesis(string sentence)
    {
      Stack<char> openParenthesis = new();

      foreach (char character in sentence)
      {
        if (character == '(')
        {
          openParenthesis.Push(character);
        }
        else if (character == ')')
        {
          if (openParenthesis.Count == 0)
          {
            return false;
          }
          char head = openParenthesis.Pop();
        }
      }

      return openParenthesis.Count == 0;
    }

    /*
     * Esta funcion se va a encargar de ver que comando sql se va a ejecutar
     * Se encarga de guardar en Data la base de datos en el instante
     */
    public static OperationStatus Parse(string sentence)
    {
      string pattern;
      if (sentence.StartsWith("CREATE DATABASE"))
      {
        pattern = @"CREATE\s+DATABASE\s(\S+)";
        Match matchDatabaseName = Regex.Match(sentence, pattern, RegexOptions.IgnoreCase);

        if (!matchDatabaseName.Success)
        {
          return OperationStatus.Error;
        }

        string databaseName = matchDatabaseName.Groups[1].Value;
        data.CreateDatabase(databaseName);
      }
      else if (sentence.StartsWith("SET DATABASE"))
      {
        pattern = @"SET\s+DATABASE\s(\S+)";
        Match matchDatabaseName = Regex.Match(sentence, pattern, RegexOptions.IgnoreCase);

        if (!matchDatabaseName.Success)
        {
          return OperationStatus.Error;
        }

        string databaseName = matchDatabaseName.Groups[1].Value;
        Console.WriteLine($"DatabaseName: {databaseName}");
        data.SetDatabase(databaseName);
      }
      else if (sentence.StartsWith("CREATE TABLE"))
      {
        string patternTableName = @"CREATE\s+TABLE\s+(\w+)";
        string patternTableContent = @"CREATE\s+TABLE\s+\w+\s*\(((?:[^()]|\((?:[^()]|\([^()]*\))*\))*)\)";
        Match matchTableName = Regex.Match(sentence, patternTableName);
        Match matchTableContent = Regex.Match(sentence, patternTableContent);

        if (!(matchTableName.Success && matchTableContent.Success))
        {
          return OperationStatus.Error;
        }

        string name = matchTableName.Groups[1].Value;
        string content = matchTableContent.Groups[1].Value;
        Console.WriteLine($"Content: {content}");
        string[] columns = content.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        data.CreateTableAs(name, columns);
      }
      else if (sentence.StartsWith("CREATE INDEX"))
      {
        pattern = @"CREATE\s+INDEX\s+(\w+)\s+ON\s+(\w+)\s*\((\w+)\)(?:\s+OF\s+TYPE\s+(\w+))?";
        Match match = Regex.Match(sentence, pattern);
        if (!match.Success)
        {
          return OperationStatus.Error;
        }
        string tableName = match.Groups[2].Value;
        string column = match.Groups[3].Value;
        string indexType = match.Groups[4].Success ? match.Groups[4].Value : "none";
        data.CreateIndex(indexType, tableName, column);
      }
      else if (sentence.StartsWith("INSERT"))
      {
        pattern = @"INSERT\s+INTO\s+(\w+)\s*VALUES\s*\(([^)]+)\)";
        Match match = Regex.Match(sentence, pattern);
        if (!match.Success)
        {
          return OperationStatus.Error;
        }
        string tableName = match.Groups[1].Value;
        string columns = match.Groups[2].Value;
        string[] columnsArray = columns.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine($"Count: {data.Table.TableFields.Count}");
        data.Insert(tableName, columnsArray);
      }
      else if (sentence.StartsWith("SELECT"))
      {
        pattern = @"SELECT ([\w\s,*]+) FROM (\w+) (WHERE ([\w\s=]+))? (ORDER BY ([\w\s]+(\s+ASC|DESC)?))?";

        Match match = Regex.Match(sentence, pattern);
        if (!match.Success)
        {
          return OperationStatus.Error;
        }
        string column = match.Groups[1].Value.Trim();
        string tableName = match.Groups[2].Value;
        string whereClause = match.Groups[3].Success ? match.Groups[4].Value.Trim() : null;
        string orderBy = match.Groups[5].Success ? match.Groups[6].Value.Trim() : null;

        // Extraer el valor de comparación de la cláusula WHERE
        string whereValue = null;
        if (whereClause != null)
        {
            Match whereMatch = Regex.Match(whereClause, @"(\w+)\s*=\s*(\w+)");
            if (whereMatch.Success)
            {
                string whereColumn = whereMatch.Groups[1].Value;
                whereValue = whereMatch.Groups[2].Value;
                Console.WriteLine($"Columna WHERE: {whereColumn}");
                Console.WriteLine($"Valor WHERE: {whereValue}");
            }
        }

        Console.WriteLine($"WHERE: {whereClause}");

        // Extraer la palabra después de LIKE
        string likeWord = null;
        if (whereClause != null)
        {
          Match likeMatch = Regex.Match(whereClause, @"LIKE\s+(\w+)"); // Updated pattern
          if (likeMatch.Success)
          {
            likeWord = likeMatch.Groups[1].Value;
          }
        }

        pattern = @"ORDER\s+BY\s+(\w+)(?:\s+(ASC|DESC))?";
        match = Regex.Match(sentence, pattern);
        string orderDirection = null;
        if (match.Success)
        {
          orderBy = match.Groups[1].Value;
          orderDirection = match.Groups[2].Success ? match.Groups[2].Value : null;
        }
        bool isAscending = true;
        if (orderDirection != null)
        {
          isAscending = orderDirection.ToUpper() == "ASC";
        }
        data.Select(tableName, column, whereClause, likeWord, orderBy, isAscending, whereValue);
      }
      else if (sentence.StartsWith("DELETE")) 
      { 
        pattern = @"DELETE\s+FROM\s+(\w+)\s+(WHERE\s+([\w\s=]+(?:'[^']*')?))?";
        Match match = Regex.Match(sentence, pattern);
        if (!match.Success)
        {
          return OperationStatus.Error;
        }
        string tableName = match.Groups[1].Value;
        string whereClause = match.Groups[2].Success ? match.Groups[3].Value.Trim() : null;

        // Extraer el valor de la cláusula WHERE
        string whereValue = null;
        if (whereClause != null)
        {
            Match whereMatch = Regex.Match(whereClause, @"(\w+)\s*=\s*'?([^']+)'?");
            if (whereMatch.Success)
            {
                string whereColumn = whereMatch.Groups[1].Value;
                whereValue = whereMatch.Groups[2].Value;
            }
        }

        data.Delete(tableName, whereClause);
      }
      else if (sentence.StartsWith("UPDATE")) 
      { 
        pattern = @"UPDATE\s+(\w+)\s+SET\s+([\w\s]+)\s*=\s*([^'\s]+|'[^']+')\s+WHERE\s+([\w\s]+)\s*=\s*([^'\s]+|'[^']+')";
        Match match = Regex.Match(sentence, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
        {
          return OperationStatus.Error;
        }
        string tableName = match.Groups[1].Value;
        string updateColumn = match.Groups[2].Value.Trim();
        string updateValue = match.Groups[3].Value.Trim('\'').Trim('"');
        string whereColumn = match.Groups[4].Value.Trim();
        string whereValue = match.Groups[5].Value.Trim('\'').Trim('"');

        Console.WriteLine($"Tabla: {tableName}");
        Console.WriteLine($"Columna a actualizar: {updateColumn}");
        Console.WriteLine($"Nuevo valor: {updateValue}");
        Console.WriteLine($"Columna WHERE: {whereColumn}");
        Console.WriteLine($"Valor WHERE: {whereValue}");

        data.Update(tableName, updateColumn, updateValue, whereColumn, whereValue);
      }
      else if (sentence.StartsWith("DROP TABLE"))
      {
        pattern = @"DROP\s+TABLE\s(\S+)";
        Match matchTableName = Regex.Match(sentence, pattern);
        if (!matchTableName.Success)
        {
          return OperationStatus.Error;
        }
        string tableName = matchTableName.Groups[1].Value;
        data.DropTable(tableName);
      }
      return OperationStatus.Success;
    }

  }
}