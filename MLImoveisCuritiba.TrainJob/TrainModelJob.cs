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

        //Divide o dataset em treino e teste.
        //TrainSet → usado para treinar o modelo | TestSet → usado para avaliar se o modelo generaliza bem.
        var split = ml.Data.TrainTestSplit(data, testFraction: 0.2);


        var pipeline = BuildPipelineHashEncoding(ml);

        //Treina o pipeline e gera o modelo final.
        var model = pipeline.Fit(split.TrainSet);

        //Aplica o modelo treinado e gera previsões. 
        var preds = model.Transform(split.TestSet);

        // Avalia a qualidade do modelo usando o TestSet.
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


    private IEstimator<ITransformer> BuildPipelineEncoding(MLContext ml)
    {
        /*
            OneHotEncoding () -> Transforma valores categóricos (strings) em vetores binários.
            Cria uma coluna para cada valor único.
            Aprende essas categorias durante o treino
         */

        return ml.Transforms.Categorical.OneHotEncoding("CidadeEnc", nameof(ModelInput.Cidade))
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
            /*
               FastForest() (FastForestRegression)
                O que é: um algoritmo de regressão baseado em Random Forest (um conjunto de árvores de decisão).
                Intuição rápida:
                Ele cria várias árvores
                Cada árvore dá uma previsão
                A previsão final é uma combinação (média) das árvores
                Por que é bom aqui:
                lida bem com relações não-lineares (ex: área × bairro × vagas)
                costuma funcionar bem sem muita engenharia complexa
             */
    }

    private IEstimator<ITransformer> BuildPipelineHashEncoding(MLContext ml)
    {
        // Pipeline com HashEncoding para categorias "abertas"
        // - Cidade/Bairro/CEP podem ter valores novos com o tempo
        // OneHotEncoding para categorias "fechadas" (SIM/NAO, casa/apartamento)

        return
        ml.Transforms.Categorical.OneHotHashEncoding(
            outputColumnName: "CidadeEnc",
            inputColumnName: nameof(ModelInput.Cidade),
            numberOfBits: 8) // 2^8 = 256 buckets

        .Append(ml.Transforms.Categorical.OneHotHashEncoding(
            outputColumnName: "BairroEnc",
            inputColumnName: nameof(ModelInput.Bairro),
            numberOfBits: 10)) // 2^10 = 1024 buckets

        .Append(ml.Transforms.Categorical.OneHotHashEncoding(
            outputColumnName: "CepEnc",
            inputColumnName: nameof(ModelInput.CEP),
            numberOfBits: 10)) // 1024 buckets

        .Append(ml.Transforms.Categorical.OneHotEncoding(
            outputColumnName: "PiscinaEnc",
            inputColumnName: nameof(ModelInput.Piscina)))

        .Append(ml.Transforms.Categorical.OneHotEncoding(
            outputColumnName: "TipoEnc",
            inputColumnName: nameof(ModelInput.Tipo)))

        .Append(ml.Transforms.Categorical.OneHotEncoding(
            outputColumnName: "ProxParqueEnc",
            inputColumnName: nameof(ModelInput.ProxParque)))

        // (Opcional mas recomendado) normalização para numéricos com escalas muito diferentes
        .Append(ml.Transforms.NormalizeMinMax(nameof(ModelInput.AreaM2)))
        .Append(ml.Transforms.NormalizeMinMax(nameof(ModelInput.CondominioMensal)))
        .Append(ml.Transforms.NormalizeMinMax(nameof(ModelInput.DistCentroKm)))

        .Append(ml.Transforms.Concatenate("Features",
            "CidadeEnc", "BairroEnc", "CepEnc",
            "PiscinaEnc", "TipoEnc", "ProxParqueEnc",
            nameof(ModelInput.QtdQuartos),
            nameof(ModelInput.AnoConstrucao),
            nameof(ModelInput.AnoReferencia),
            nameof(ModelInput.AreaM2),
            nameof(ModelInput.VagasGaragem),
            nameof(ModelInput.CondominioMensal),
            nameof(ModelInput.DistCentroKm)
        ))
        .Append(ml.Regression.Trainers.FastForest(
            labelColumnName: "Label",
            featureColumnName: "Features"));


        /*
            Escolha dos numberOfBits (bem prático)
            Cidade (provavelmente pouca variação) → 8 (256)
            Bairro (mais variação) → 10 (1024)
            CEP (muita variação) → 10 ou 12 (4096) se você começar a usar muitos CEPs
            Se você notar queda de performance por colisões (raramente em POC), aumente numberOfBits.
         */
    }
}
