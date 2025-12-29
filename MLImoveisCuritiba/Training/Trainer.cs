namespace MLImoveisCuritiba.Training;

using Microsoft.ML;
using static MLImoveisCuritiba.ImovelVenda;

public static class Trainer
{
    public static ITransformer TrainAndSave(MLContext ml, string csvPath, string modelPath)
    {
        var data = ml.Data.LoadFromTextFile<ModelInput>(
            path: csvPath, hasHeader: true, separatorChar: ',');

        var split = ml.Data.TrainTestSplit(data, testFraction: 0.2);

        // Pipeline de processamento
        var dataProcess =
            ml.Transforms.ReplaceMissingValues(nameof(ModelInput.AreaM2))
              .Append(ml.Transforms.ReplaceMissingValues(nameof(ModelInput.CondominioMensal)))
              .Append(ml.Transforms.ReplaceMissingValues(nameof(ModelInput.DistCentroKm)))

              // Categóricas -> OneHot
              .Append(ml.Transforms.Categorical.OneHotEncoding("CidadeEnc", nameof(ModelInput.Cidade)))
              .Append(ml.Transforms.Categorical.OneHotEncoding("BairroEnc", nameof(ModelInput.Bairro)))
              .Append(ml.Transforms.Categorical.OneHotEncoding("EstadoEnc", nameof(ModelInput.Estado)))
              .Append(ml.Transforms.Categorical.OneHotEncoding("PiscinaEnc", nameof(ModelInput.Piscina)))
              .Append(ml.Transforms.Categorical.OneHotEncoding("TipoEnc", nameof(ModelInput.Tipo)))
              .Append(ml.Transforms.Categorical.OneHotEncoding("ProxParqueEnc", nameof(ModelInput.ProxParque)))

              // (Opcional) Normalizar numéricos
              .Append(ml.Transforms.NormalizeMinMax("QtdQuartosN", nameof(ModelInput.QtdQuartos)))
              .Append(ml.Transforms.NormalizeMinMax("AnoConstrucaoN", nameof(ModelInput.AnoConstrucao)))
              .Append(ml.Transforms.NormalizeMinMax("AnoReferenciaN", nameof(ModelInput.AnoReferencia)))
              .Append(ml.Transforms.NormalizeMinMax("AreaM2N", nameof(ModelInput.AreaM2)))
              .Append(ml.Transforms.NormalizeMinMax("VagasGaragemN", nameof(ModelInput.VagasGaragem)))
              .Append(ml.Transforms.NormalizeMinMax("CondominioMensalN", nameof(ModelInput.CondominioMensal)))
              .Append(ml.Transforms.NormalizeMinMax("DistCentroKmN", nameof(ModelInput.DistCentroKm)))

              // Features
              .Append(ml.Transforms.Concatenate("Features",
                  "CidadeEnc", "BairroEnc", "EstadoEnc",
                  "PiscinaEnc", "TipoEnc", "ProxParqueEnc",
                  "QtdQuartosN", "AnoConstrucaoN", "AnoReferenciaN",
                  "AreaM2N", "VagasGaragemN", "CondominioMensalN", "DistCentroKmN"
              ));

        // Trainer (você pode trocar por LightGbm também)
        var trainer = ml.Regression.Trainers.FastForest(labelColumnName: "Label", featureColumnName: "Features");
        var pipeline = dataProcess.Append(trainer);

        var model = pipeline.Fit(split.TrainSet);

        // Avaliar
        var preds = model.Transform(split.TestSet);
        var metrics = ml.Regression.Evaluate(preds, labelColumnName: "Label");

        Console.WriteLine($"R²:   {metrics.RSquared:0.####}");
        Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError:0.##}");
        Console.WriteLine($"MAE:  {metrics.MeanAbsoluteError:0.##}");

        // Salvar modelo
        ml.Model.Save(model, split.TrainSet.Schema, modelPath);
        Console.WriteLine($"Modelo salvo em: {modelPath}");

        return model;
    }
}

