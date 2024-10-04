namespace ParserTests;

using ApiInterface.Parser;
using ApiInterface.Models;
using Xunit;

public class SQLQueryProcessorTests
{
  [Fact]
  public void AddSentences_SingleSentence_ShouldAddOneSentence()
  {
    string script = "SELECT * FROM Users;";

    // Act
    List<string> Sentences = SQLQueryProcessor.AddSentences(script);

    // Assert
    Assert.Single(Sentences);
    Assert.Equal("SELECT * FROM Users", Sentences[0]);
  }

  [Fact]
  public void AddSentences_MultipleSentences_ShouldAddAllSentences()
  {
    string script = "SELECT * FROM Users; INSERT INTO Users (Name) VALUES ('John'); DELETE FROM Users WHERE Id = 1;";

    // Act
    List<string> Sentences = SQLQueryProcessor.AddSentences(script);

    // Assert
    Assert.Equal(3, Sentences.Count);
    Assert.Equal("SELECT * FROM Users", Sentences[0]);
    Assert.Equal("INSERT INTO Users (Name) VALUES ('John')", Sentences[1]);
    Assert.Equal("DELETE FROM Users WHERE Id = 1", Sentences[2]);
  }

  [Fact]
  public void AddSentences_ExtraWhitespacesAndNewlines_ShouldTrimSentences()
  {
    string script = @"  SELECT * FROM Users;
                            INSERT INTO Users (Name) VALUES ('John');
                            DELETE FROM Users WHERE Id = 1;
                            CREATE TABLE ESTUDIANTES(
                              ID INTEGER,
                              NAME VARCHAR(255)
                            );
    ";

    // Act
    List<string> Sentences = SQLQueryProcessor.AddSentences(script);

    // Assert
    Assert.Equal(4, Sentences.Count);
    Assert.Equal("SELECT * FROM Users", Sentences[0]);
    Assert.Equal("INSERT INTO Users (Name) VALUES ('John')", Sentences[1]);
    Assert.Equal("DELETE FROM Users WHERE Id = 1", Sentences[2]);
    Assert.Equal("CREATE TABLE ESTUDIANTES( ID INTEGER, NAME VARCHAR(255) )", Sentences[3]);
  }

  [Fact]
  public void AddSentences_EmptyScript_ShouldNotAddSentences()
  {
    string script = "";

    // Act
    List<string> Sentences = SQLQueryProcessor.AddSentences(script);

    // Assert
    Assert.Empty(Sentences);
  }

  [Fact]
  public void AddSentences_OnlyWhitespaces_ShouldNotAddSentences()
  {
    string script = "   ";

    // Act
    List<string> Sentences = SQLQueryProcessor.AddSentences(script);

    // Assert
    Assert.Empty(Sentences);
  }

  [Fact]
  public void HasCorrectParenthesis_True()
  {
    string[] correctSentences = { "(())", "()" };
    foreach (string sentence in correctSentences)
    {
      Assert.Equal(SQLQueryProcessor.HasCorrectParenthesis(sentence), true);
    }
  }

  [Fact]
  public void HasCorrectParenthesis_False()
  {
    string[] wrongSentences = { "(()", "(", "())" };
    foreach (string sentence in wrongSentences)
    {
      Assert.Equal(SQLQueryProcessor.HasCorrectParenthesis(sentence), false);
    }
  }

  [Fact]
  public void Parse_CreateDatabaseSuccess()
  {
    string[] sentences = { "CREATE DATABASE ESTUDIANTES", "CREATE DATABASE PROFESORES", "CREATE DATABASE 1CONTRASEÑAS " };
    foreach (string sentence in sentences)
    {
      OperationStatus parsed = SQLQueryProcessor.Parse(sentence);
      Assert.Equal(parsed, OperationStatus.Success);
    }
  }

  [Fact]
  public void Parse_CreateDatabaseError()
  {
    string[] sentences = { "CREATE DATABASE ", "CREATE DATABASE       ", "CREATE DATABASE" };
    foreach (string sentence in sentences)
    {
      OperationStatus parsed = SQLQueryProcessor.Parse(sentence);
      Assert.Equal(parsed, OperationStatus.Error);
    }
  }
}
