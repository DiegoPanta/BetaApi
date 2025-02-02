using Amazon.DynamoDBv2;
using Amazon.SQS;
using Aplication.LoanSimulation.Commands;
using Domain.Business;
using Infrastructure.ExternalServices;
using Infrastructure.Repositories;
using Interfaces.IExternalService;
using Interfaces.IRepositories;
using MediatR;

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

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
        }

        // Usando a política que você criou
        app.UseCors("AllowFrontend"); 

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}