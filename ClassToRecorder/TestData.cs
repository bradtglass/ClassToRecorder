namespace ClassToRecorder;

public class TestData
{
    public const string TestA = """
                                using Brain.Api.Handlers;
                                using Brain.Api.Handlers.Client.Create;
                                using Brain.Api.Response.Client;
                                using Brain.FinancialData.Enums;
                                using MediatR;

                                namespace Brain.Api.Request.Client;
                                
                                public class CreateClientRequest : BaseClientRequest, IRequest<HandlerPayloadResponse<CreateClientResponseStatus, ClientResponse?>>
                                {
                                    public ClientRating Rating { get; init; }
                                    public PaymentTerms PaymentTerms { get; init; }
                                }
                                
                                public abstract class AnotherType<T> where T : notnull
                                {
                                    public ClientRating Rating { get; init; }
                                    private double Hello { get; init; }
                                    public PaymentTerms PaymentTerms { get => 0; }
                                    public void HelloWorld()
                                    {
                                        Console.WriteLine(""Hello World"");
                                    }
                                }
                                """;
    
    public const string TestB = """
                                namespace Brain.Api.Request.Client
                                {

                                    using Brain.Api.Handlers;
                                    using Brain.Api.Handlers.Client.Create;
                                    using Brain.Api.Response.Client;
                                    using Brain.FinancialData.Enums;
                                    using MediatR;

                                    
                                    public class CreateClientRequest : BaseClientRequest, IRequest<HandlerPayloadResponse<CreateClientResponseStatus, ClientResponse?>>
                                    {
                                        public ClientRating Rating { get; init; }
                                        public PaymentTerms PaymentTerms { get; init; }
                                    }
                                }
                                """;
}
