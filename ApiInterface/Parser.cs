/*
 * TODO: 
 *    (3) Completar la funcion que haga el parse de cada comando individual
 */
using System.Text.RegularExpressions;
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
      Sentences = AddSentences(script);
      OperationStatus result = OperationStatus.Success;
      if (Sentences.Count == 0)
      {
        return OperationStatus.Error;
      }

      foreach (string Sentence in Sentences)
      {
        if (Sentence.Contains("(") || Sentence.Contains(")") && !HasCorrectParenthesis(Sentence))
        {
          return OperationStatus.Error;
        }
        result = Parse(Sentence);
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
          NewSentences.Add(sentence.Trim());
        }
      }
      return NewSentences;
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
      }
      else if (sentence.StartsWith("SET DATABASE")) { }
      else if (sentence.StartsWith("CREATE TABLE")) { }
      else if (sentence.StartsWith("CREATE INDEX")) { }
      else if (sentence.StartsWith("INSERT")) { }
      else if (sentence.StartsWith("SELECT")) { }
      else if (sentence.StartsWith("DELETE")) { }
      else if (sentence.StartsWith("UPDATE SET")) { }
      return OperationStatus.Success;
    }

  }
}
