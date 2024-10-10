/*
 * TODO: 
 *    (3) Completar la funcion que haga el parse de cada comando individual
 */
using System.Text.RegularExpressions;
using System.Diagnostics;
using ApiInterface.Models;
using ApiInterface.Store;
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
      Stopwatch stopwatch = new();
      Console.WriteLine(script);
      if (Sentences.Count == 0)
      {
        return OperationStatus.Error;
      }

      foreach (string Sentence in Sentences)
      {
        Console.WriteLine($"PARSE: {Sentence}");
        if ((Sentence.Contains("(") || Sentence.Contains(")")) && !HasCorrectParenthesis(Sentence))
        {
          return OperationStatus.Error;
        }
        stopwatch.Start();
        result = Parse(Sentence);
        stopwatch.Stop();
        Console.WriteLine($"TIME: {stopwatch.ElapsedMilliseconds}");
        if (result == OperationStatus.Warning || result == OperationStatus.Error)
        {
          return result;
        }
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
      sentence = sentence.ToUpper();
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
        data.SetDatabaseAs(databaseName);
      }
      else if (sentence.StartsWith("CREATE TABLE"))
      {
        string patternTableName = @"CREATE\s+TABLE\s+(\w+)";
        string patternTableContent = @"CREATE\s+TABLE\s+\w+\s*\(([^)]+)\)";
        Match matchTableName = Regex.Match(sentence, patternTableName);
        Match matchTableContent = Regex.Match(sentence, patternTableContent);

        if (!(matchTableName.Success && matchTableContent.Success))
        {
          return OperationStatus.Error;
        }

        string name = matchTableName.Groups[1].Value;
        string content = matchTableContent.Groups[1].Value;
        string[] columns = content.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
      }
      else if (sentence.StartsWith("CREATE INDEX")) { }
      else if (sentence.StartsWith("INSERT")) { }
      else if (sentence.StartsWith("SELECT")) { }
      else if (sentence.StartsWith("DELETE")) { }
      else if (sentence.StartsWith("UPDATE SET")) { }
      else if (sentence.StartsWith("DROP TABLE"))
      {
        pattern = @"DROP\s+TABLE\s(\S+)";
        Match matchTableName = Regex.Match(sentence, pattern);
        if (!matchTableName.Success)
        {
          return OperationStatus.Error;
        }
      }
      return OperationStatus.Success;
    }

  }
}
