using Microsoft.Extensions.ML;
using Microsoft.OpenApi;
using MLImoveisCuritiba.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fundamentos ML .NET",
        Version = "v1"
    });
});

/*
Atenção: “onde o modelo fica” (muito importante)
Se sua WebApi e o Job rodam:
na mesma máquina: fácil, mesma pasta (C:/Models).
em Docker/K8s: use um volume compartilhado.
em Azure App Service: filesystem pode ser limitado; aí normalmente usa Blob Storage e a API baixa o modelo.
*/
var modelPath = "C:/Models/ImovelVenda.mlnet";

//PredictionEnginePool é o jeito certo pra Web API (thread-safe).
builder.Services.AddPredictionEnginePool<ModelInput, ModelOutput>()
    .FromFile(modelName: "ImovelModel", filePath: modelPath, watchForChanges: true); //watchForChanges: true recarrega quando o arquivo do modelo muda.




var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
