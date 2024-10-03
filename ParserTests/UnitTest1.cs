namespace ParserTests;

using ApiInterface.Parser;
using System;
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
    string script = "  SELECT * FROM Users;  \n  INSERT INTO Users (Name) VALUES ('John');\n  DELETE FROM Users WHERE Id = 1; ";

    // Act
    List<string> Sentences = SQLQueryProcessor.AddSentences(script);

    // Assert
    Assert.Equal(3, Sentences.Count);
    Assert.Equal("SELECT * FROM Users", Sentences[0]);
    Assert.Equal("INSERT INTO Users (Name) VALUES ('John')", Sentences[1]);
    Assert.Equal("DELETE FROM Users WHERE Id = 1", Sentences[2]);
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

}
