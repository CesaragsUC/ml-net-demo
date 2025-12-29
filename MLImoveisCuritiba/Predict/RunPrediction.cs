using Microsoft.ML;
using MLImoveisCuritiba.Predict;
using MLImoveisCuritiba.Training;
using static MLImoveisCuritiba.ImovelVenda;

namespace MLImoveisCuritibaApp.Predict;


/// <summary>
/// Treinar modelo sem usar UI do Model Builder
/// Treino (offline) → gera ImovelVenda.mlnet
/// Predição (online) → só carrega o .mlnet e faz Predict()
/// </summary>
internal static class RunPrediction
{
    public static void Run()
    {
        var ml = new MLContext(seed: 42);

        // Pega pasta do .csproj (raiz do projeto)
        var projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

        var csvPath = Path.Combine(projectRoot, "Data", "curitiba_imoveis_ml_poc_v2.csv");
        var modelPath = Path.Combine(projectRoot, "ImovelVenda.mlnet");

        // 1) Treinar e salvar (offline)
        Trainer.TrainAndSave(ml, csvPath, modelPath);

        // 2) Predizer (online)
        var sample = new ModelInput
        {
            Cidade = "Curitiba",
            Bairro = "Batel",
            Estado = "PR",
            CEP = "80420-000",
            QtdQuartos = 3,
            Piscina = "NAO",
            Tipo = "apartamento",
            AnoConstrucao = 2018,
            AnoReferencia = 2025, // ano corrente controlado aqui
            AreaM2 = 110,
            VagasGaragem = 2,
            CondominioMensal = 1500,
            DistCentroKm = 2.0f,
            ProxParque = "SIM",
        };

        var result = Predictor.Predict(ml, modelPath, sample);
        Console.WriteLine($"Preço previsto (R$): {result.Score:n2}");

        Console.ReadKey();
    }
}
