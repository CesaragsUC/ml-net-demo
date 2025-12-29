using Microsoft.ML;
using static MLImoveisCuritiba.ImovelVenda;

namespace MLImoveisCuritiba.Predict;

public static class Predictor
{
    public static ModelOutput Predict(MLContext ml, string modelPath, ModelInput input)
    {
        var model = ml.Model.Load(modelPath, out _);
        var engine = ml.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
        return engine.Predict(input);
    }
}
