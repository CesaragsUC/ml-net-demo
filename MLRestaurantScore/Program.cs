using System;

namespace MLRestaurantScore;

/// <summary>
/// Exemplos de DataSet para testes: https://github.com/dotnet/machinelearning-samples
/// Exemplo base: https://learn.microsoft.com/en-us/dotnet/machine-learning/tutorials/predict-prices-with-model-builder
/// </summary>

class Program
{
    static void Main(string[] args)
    {
        //// recebe dados de amostra para prever falha de máquina
        //Load sample data
        var sampleData = new RestaurantViolationsPrediction.ModelInput()
        {
            InspectionType = @"Reinspection/Followup",
            ViolationDescription = @"did not wash hands when handling food.",
        };


        // Util para obter todas as probabilidades de classes
        var scoreWithlabels = RestaurantViolationsPrediction.PredictAllLabels(sampleData);

        Console.WriteLine($"{"Class",-40} {"Score",-20}");
        Console.WriteLine($"{"------",-40} {"------",-20}");

        foreach (var score in scoreWithlabels)
        {
            var probabilidadeFormatada = FormatarProbabilidade(score.Value);

            Console.WriteLine($"{score.Key,-40}{probabilidadeFormatada,-20}");
        }

        //** Predict() Útil quando você só quer saber: vai falhar ou não?**
        var result = RestaurantViolationsPrediction.Predict(sampleData);

        Console.WriteLine("\n\n Single prediction result.");
        Console.WriteLine($"InspectionType: {result.InspectionType}");
        Console.WriteLine($"ViolationDescription: {result.ViolationDescription}");
        Console.WriteLine($"RiskCategory: {result.RiskCategory}");
        Console.WriteLine($"Score: {result.Score}");


        Console.WriteLine("============= End of process =============");

        Console.ReadKey();
    }

    public static string FormatarProbabilidade(float score)
    {
        return score switch
        {
            < 0.01f => $"{score:P2}",    // 0.00% - 0.99% → 2 casas (ex: 0.13%)
            < 0.10f => $"{score:P1}",    // 1.0% - 9.9%  → 1 casa  (ex: 2.5%)
            _ => $"{score:P0}"           // ≥ 10%         → 0 casas (ex: 80%)
        };
    }
}


