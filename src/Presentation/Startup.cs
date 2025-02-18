using System.Text;
using Amazon.CloudWatchLogs;
using Amazon.DynamoDBv2;
using Amazon.SQS;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Aplication.LoanSimulation.Commands;
using Domain.Business;
using Infrastructure.ExternalServices;
using Infrastructure.Repositories;
using Interfaces.IExternalService;
using Interfaces.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;

namespace Presentation;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var key = Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization();

        // AWS CloudWatch Logging Configuration
        var logGroupName = "/aws/lambda/BetaApi";
        var cloudWatchClient = new AmazonCloudWatchLogsClient();

        var logDirectory = "logs";
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        var options = new CloudWatchSinkOptions
        {
            LogGroupName = logGroupName,
            TextFormatter = new JsonFormatter(),
            MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information,
            BatchSizeLimit = 10,
            Period = TimeSpan.FromSeconds(2),
            CreateLogGroup = true
        };

        // Configure Serilog for JSON Structured Logging
        Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Information()
             .Enrich.FromLogContext()
             .WriteTo.Console(new CompactJsonFormatter())  // Console output as JSON
             .WriteTo.File(
                 path: Path.Combine(logDirectory, "app_log.json"),
                 rollingInterval: RollingInterval.Day,
                 retainedFileCountLimit: 7,
                 fileSizeLimitBytes: 10_000_000,
                 flushToDiskInterval: System.TimeSpan.FromSeconds(5),
                 formatter: new CompactJsonFormatter())  // Proper JSON formatting
             .CreateLogger();

        //Configuração cloudwatch
        services.AddAWSService<IAmazonCloudWatchLogs>();
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog();
        });

        // Enable AWS X-Ray
        AWSSDKHandler.RegisterXRayForAllServices();

        //adicionando dynamoDb
        services.AddAWSService<IAmazonDynamoDB>();

        // Adicionar serviços
        services.AddMediatR(typeof(SimulateLoanHandler).Assembly);
        services.AddSwaggerGen();
        services.AddSingleton<LoanCalculator>();
        services.AddScoped<ILoanSimulationRepository, LoanSimulationRepository>();
        services.AddScoped<IInterestRateService, InterestRateService>();

        //Registra serviços para envio e consumo de mensagens SQS
        services.AddSingleton<ISqsService, SqsService>();
        services.AddSingleton<ISqsConsumerService, SqsConsumerService>();
        services.AddSingleton<AWSXRayRecorder>();

        //Resiliencia de serviços Polly
        services.AddHttpClient<IInterestRateService, InterestRateService>()
            .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
            .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy())
            .AddPolicyHandler(PollyPolicies.GetTimeoutPolicy());

        //Adiciona suporte ao AWS SQS
        services.AddAWSService<IAmazonSQS>();

        // Permitindo o frontend Vue
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
                builder.WithOrigins("http://localhost:5173") 
                       .AllowAnyMethod()
                       .AllowAnyHeader());
        });

        // Configuração do Controller
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Loan Calculation API v1");
                c.RoutePrefix = string.Empty;
            });
            app.UseXRay("BetaApi"); // Enables X-Ray tracing
        }

        // Add Request Logging Middleware
        app.UseMiddleware<RequestLoggingMiddleware>();

        // Usando a política que você criou
        app.UseCors("AllowFrontend"); 

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}