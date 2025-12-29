using System;

namespace MLSampleApp;

/// <summary>
/// Exemplos de DataSet para testes: https://github.com/dotnet/machinelearning-samples
/// </summary>

class Program
{
    static void Main(string[] args)
    {
        // recebe dados de amostra para prever falha de máquina
        var sampleData = new PredictiveModel.ModelInput()
        {
            UDI = 2F,
            Product_ID = @"L47181",
            Air_temperature = 298.2F,
            Process_temperature = 308.7F,
            Rotational_speed = 1408F,
            Torque = 463F,
            Tool_wear = 3F,
        };

        Console.WriteLine("Comparing actual Machine_failure with predicted machine failure from sample data ..\n\n");

        // Util para obter todas as probabilidades de classes
        var scoreWithlabels = PredictiveModel.PredictAllLabels(sampleData);

        Console.WriteLine($"{"Class",-10} {"Score",-20 }");
        Console.WriteLine($"{"------",-10} {"------",-20}");

        foreach (var score in scoreWithlabels)
        {
            var probabilidadeFormatada = FormatarProbabilidade(score.Value);

            Console.WriteLine($"{score.Key, -10}{probabilidadeFormatada, -20}");
        }

        //** Predict() Útil quando você só quer saber: vai falhar ou não?**
        var result = PredictiveModel.Predict(sampleData);

        Console.WriteLine("\n\n Single prediction result.");
        Console.WriteLine($"UDI: {result.UDI}");
        Console.WriteLine($"Product_ID: {result.Product_ID}");
        Console.WriteLine($"Air_temperature: {result.Air_temperature}");
        Console.WriteLine($"Process_temperature: {result.Process_temperature}");
        Console.WriteLine($"Rotational_speed: {result.Rotational_speed}");
        Console.WriteLine($"Torque: {result.Torque}");
        Console.WriteLine($"Tool_wear: {result.Tool_wear}");
        Console.WriteLine($"Machine_failure: {result.Machine_failure}");// resultado aqui (1) falha e (0) nao falha


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


