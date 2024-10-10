using ApiInterface.Exceptions;
using ApiInterface.Parser;
using ApiInterface.Models;

namespace ApiInterface.Processor
{
  internal interface IProcessor
  {
    Response Process();
  }
  internal class ProcessorFactory
  {
    internal static IProcessor Create(Request request)
    {
      return new SQLSentenceProcessor(request);
    }
  }
  internal class SQLSentenceProcessor(Request request) : IProcessor
  {
    public Request Request { get; } = request;

    public Response Process()
    {
      var sentence = this.Request.Body;
      var result = SQLQueryProcessor.Execute(sentence);
      var response = this.ConvertToResponse(result);
      return response;
    }

    private Response ConvertToResponse(OperationStatus result)
    {
      return new Response
      {
        Status = result,
        Request = this.Request,
        ResponseBody = "Success"
      };
    }
  }
}
