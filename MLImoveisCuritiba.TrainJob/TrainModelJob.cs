namespace MLImoveisCuritiba.TrainJob;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using MLImoveisCuritiba.Shared.Models;
using Quartz;

[DisallowConcurrentExecution] // evita rodar duas vezes em paralelo
public class TrainModelJob : IJob
{
    private readonly ILogger<TrainModelJob> _logger;
    private readonly IConfiguration _config;

    public TrainModelJob(ILogger<TrainModelJob> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public Task Execute(IJobExecutionContext context)
    {
        // Pega pasta do .csproj (raiz do projeto)
        var projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

        var csvPath = _config["Training:CsvPath"]!;
        var finalModelPath = _config["Training:ModelPath"]!;

        // caminho absoluto a partir do diretório do executável do job
        if (!Path.IsPathRooted(csvPath))
            csvPath = Path.Combine(projectRoot, csvPath);

        Directory.CreateDirectory(Path.GetDirectoryName(finalModelPath)!);

        _logger.LogInformation("Treino iniciado. CSV: {Csv} | Modelo: {Model}", csvPath, finalModelPath);

        var ml = new MLContext(seed: 42);

        var data = ml.Data.LoadFromTextFile<ModelInput>(
            path: csvPath,
            hasHeader: true,
            separatorChar: ',');

        var split = ml.Data.TrainTestSplit(data, testFraction: 0.2);

        // pipeline (pode ajustar)
        var pipeline =
            ml.Transforms.Categorical.OneHotEncoding("CidadeEnc", nameof(ModelInput.Cidade))
            .Append(ml.Transforms.Categorical.OneHotEncoding("BairroEnc", nameof(ModelInput.Bairro)))
            .Append(ml.Transforms.Categorical.OneHotEncoding("EstadoEnc", nameof(ModelInput.Estado)))
            .Append(ml.Transforms.Categorical.OneHotEncoding("PiscinaEnc", nameof(ModelInput.Piscina)))
            .Append(ml.Transforms.Categorical.OneHotEncoding("TipoEnc", nameof(ModelInput.Tipo)))
            .Append(ml.Transforms.Categorical.OneHotEncoding("ProxParqueEnc", nameof(ModelInput.ProxParque)))
            .Append(ml.Transforms.Concatenate("Features",
                "CidadeEnc", "BairroEnc", "EstadoEnc", "PiscinaEnc", "TipoEnc", "ProxParqueEnc",
                nameof(ModelInput.QtdQuartos),
                nameof(ModelInput.AnoConstrucao),
                nameof(ModelInput.AnoReferencia),
                nameof(ModelInput.AreaM2),
                nameof(ModelInput.VagasGaragem),
                nameof(ModelInput.CondominioMensal),
                nameof(ModelInput.DistCentroKm)
            ))
            .Append(ml.Regression.Trainers.FastForest(labelColumnName: "Label", featureColumnName: "Features"));

        var model = pipeline.Fit(split.TrainSet);

        var preds = model.Transform(split.TestSet);
        var metrics = ml.Regression.Evaluate(preds, labelColumnName: "Label");

        _logger.LogInformation("Métricas: R²={R2} RMSE={Rmse} MAE={Mae}",
            metrics.RSquared, metrics.RootMeanSquaredError, metrics.MeanAbsoluteError);

        // salvar com atomic replace (evita API ler arquivo incompleto)
        var tempPath = finalModelPath + ".tmp";
        ml.Model.Save(model, split.TrainSet.Schema, tempPath);

        File.Copy(tempPath, finalModelPath, overwrite: true);
        File.Delete(tempPath);

        _logger.LogInformation("Modelo atualizado: {Model}", finalModelPath);

        return Task.CompletedTask;
    }
}
