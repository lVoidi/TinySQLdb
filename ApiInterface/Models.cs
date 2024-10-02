namespace ApiInterface.Models
{
  internal enum RequestType
  {
    SQLSentence = 0
  }

  internal class Request
  {
    public required RequestType Type { get; set; }
    public required string Body { get; set; }
  }

  internal class Response
  {
    public required Request Request { get; set; }
    public required OperationStatus Status { get; set; }
    public required string ResponseBody { get; set; }
  }

  public enum OperationStatus
  {
    Success,
    Error,
    Warning
  }

  public enum TableIndex
  {
    None,
    BSTree,
    BTree
  }

}
