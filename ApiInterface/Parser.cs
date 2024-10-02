/*
 * TODO: 
 *    (1) Completar la funcion para definir los comandos
 *    (2) Completar la funcion que compruebe los parentesis 
 *    (3) Completar la funcion que haga el parse de cada comando individual
 */
using ApiInterface.Models;
using ApiInterface.Store;
namespace ApiInterface.Parser
{
  public class SQLQueryProcessor
  {
    private static List<string> Sentences = new();
    private static Data? data;

    public static OperationStatus Execute(string script)
    {
      AddSentences(script);

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
      }

      return OperationStatus.Success;
    }

    /*
     * TODO: (1)
     * Este es el que separa las lineas de codigo por puntos y comas
     * Elimina Espacios en blanco extra, tabs y newlines
     */
    private static void AddSentences(string script)
    {
    }

    /*
     * TODO: (2)
     * Esta funcion se encarga de chequear los parentesis 
     */
    private static bool HasCorrectParenthesis(string sentence)
    {
      return true;
    }

    /*
     * TODO: (3)
     * Esta funcion se va a encargar de ver que comando sql se va a ejecutar
     */
    private static void Parse(string sentence)
    {

    }

  }
}
