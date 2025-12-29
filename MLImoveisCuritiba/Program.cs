using Microsoft.ML;
using MLImoveisCuritibaApp.Predict;
using static MLImoveisCuritiba.ImovelVenda;

namespace MLImoveisCuritiba;

/// <summary>
/// Exemplos de DataSet para testes: https://github.com/dotnet/machinelearning-samples
/// Exemplo base: https://learn.microsoft.com/en-us/dotnet/machine-learning/tutorials/predict-prices-with-model-builder
/// </summary>

/*
“Produção real”: separar treino de predição
Treino roda 1x por dia/semana/mês e atualiza ImovelVenda.mlnet
API só carrega o .mlnet e chama Predict
Assim quando você subir um novo .mlnet, a API atualiza sem restart.
 */
class Program
{
    static void Main(string[] args)
    {
        RunPrediction.Run();

        //var input = new ModelInput
        //{
        //    Cidade = "Curitiba",
        //    Bairro = "Batel",
        //    Estado = "PR",
        //    CEP = "80420-000",
        //    QtdQuartos = 3,
        //    Piscina = "NAO",
        //    Tipo = "apartamento",
        //    AnoConstrucao = 2018,
        //    AnoReferencia = 2025,
        //    AreaM2 = 110,
        //    VagasGaragem = 2,
        //    CondominioMensal = 1500,
        //    DistCentroKm = 2.0f,
        //    ProxParque = "SIM"
        //};

        //var pred = ImovelVenda.Predict(input);
        //Console.WriteLine($"Preço previsto: R$ {pred.Score:n2}");

        Console.ReadKey();
    }
}


