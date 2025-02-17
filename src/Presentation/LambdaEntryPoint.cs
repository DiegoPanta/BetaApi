using Amazon.Lambda.AspNetCoreServer;

namespace Presentation;

public class LambdaEntryPoint : APIGatewayProxyFunction
{

    protected override void Init(IWebHostBuilder builder)
    {
        builder
            .UseStartup<Startup>();
    }

    protected override void Init(IHostBuilder builder)
    {
        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
    }
}