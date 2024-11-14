// See https://aka.ms/new-console-template for more information

using Basics;
using Grpc.Core;
using Grpc.Net.Client;

Console.WriteLine("Hello, World!");
var option = new GrpcChannelOptions { };

using var channel = GrpcChannel.ForAddress("https://localhost:7086", option);
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

//UnarySample(client);
//ClientStreaming(client);
ServerStreaming(client);
Console.ReadLine();

void UnarySample(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request { Content = "Hello You!" };
    var response = client.Unary(request);
}

async Task ClientStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var call = client.ClientStream();
    for (int i = 0; i < 100; i++)
    {
        await call.RequestStream.WriteAsync(new Request() { Content = i.ToString() });
    }

    await call.RequestStream.CompleteAsync();
    Response response = await call;
    Console.WriteLine($"{response.Message}");
}

async void ServerStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var streamCall = client.ServerStream(new Request() { Content = "Hello!" });

    await foreach (var response in streamCall.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(response.Message);
    }
}

async void BidirectionalStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using (var call = client.BiDirectionalStream())
    {
        var request = new Request();
        for (int i = 0; i < 10; i++)
        {
            request.Content = i.ToString();
            Console.WriteLine(request.Content);
            await call.RequestStream.WriteAsync(request);
        }

        while (await call.ResponseStream.MoveNext())
        {
            var message = call.ResponseStream.Current;
            Console.WriteLine(message);
        }

        await call.RequestStream.CompleteAsync();
    }
}
