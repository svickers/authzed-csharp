using Authzed.Api.v0;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace examples
{
    public class Program
    {


        static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = CreateAuthenticatedChannel("https://grpc.authzed.com", "YOUR_TOKEN_HERE");
            
            var client = new Authzed.Api.v0.ACLService.ACLServiceClient(channel);
            var permission = new ObjectAndRelation()
            {
                Namespace = "headset_dev/company",
                ObjectId = "companya",
                Relation = "member"
            };
            var user = new User() { Userset = new ObjectAndRelation { Namespace = "headset_dev/user", ObjectId = "companya_admin", Relation="..." } };

            var c = await client.CheckAsync(new CheckRequest() { User = user, TestUserset = permission });
            Console.WriteLine(c.IsMember);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static GrpcChannel CreateAuthenticatedChannel(string address, string token)
        {
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    metadata.Add("Authorization", $"Bearer {token}");
                }
                return Task.CompletedTask;
            });

            // SslCredentials is used here because this channel is using TLS.
            // CallCredentials can't be used with ChannelCredentials.Insecure on non-TLS channels.
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
            return channel;
        }
    }
}
