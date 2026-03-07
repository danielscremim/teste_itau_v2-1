using ComprasProgramadas.Application.Extensions;
using ComprasProgramadas.API.Services;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 
// SERVICOS: Infrastructure (DB, Kafka, etc)
// 
builder.Services.AddInfrastructure(builder.Configuration);

// 
// SERVICOS: Application (Use Cases, Validators)
// 
builder.Services.AddApplication(builder.Configuration);

// 
// SERVICOS: Controllers + Swagger
// 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Compra Programada API",
        Version     = "v1",
        Description = "Sistema de Compra Programada de Acoes - Itau Corretora"
    });
});

// 
// SERVICOS: Agendamento automático do motor de compra (dias 5, 15, 25)
// 
builder.Services.AddHostedService<MotorCompraSchedulerService>();

var app = builder.Build();

// 
// MIDDLEWARE: Tratamento global de erros
// Transforma DomainException em HTTP 400
// e qualquer outra Exception em HTTP 500
// 
app.UseExceptionHandler(errApp => errApp.Run(async context =>
{
    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

    if (ex is DomainException domEx)
    {
        context.Response.StatusCode  = 400;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { erro = domEx.Message });
    }
    else if (ex is ValidationException valEx)
    {
        context.Response.StatusCode  = 422;
        context.Response.ContentType = "application/json";
        var erros = valEx.Errors.Select(e => new { campo = e.PropertyName, msg = e.ErrorMessage });
        await context.Response.WriteAsJsonAsync(new { erros });
    }
    else
    {
        context.Response.StatusCode  = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { erro = "Erro interno do servidor." });
    }
}));

// 
// PIPELINE
// 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
