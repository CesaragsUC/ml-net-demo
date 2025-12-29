using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using MLImoveisCuritiba.Shared.Models;
using MLImoveisCuritiba.WebApi.Models;
using System.Globalization;

namespace MLImoveisCuritiba.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/predict")]
    public class PredictPriceController : ControllerBase
    {
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _pool;

        public PredictPriceController(PredictionEnginePool<ModelInput, ModelOutput> pool)
        {
            _pool = pool;
        }

        [HttpPost()]
        [Route("price")]
        public async Task<IActionResult> PredictPrice([FromBody] PredictRequest req)
        {
            // Map request -> ModelInput (Label não importa na predição)
            var input = new ModelInput
            {
                Cidade = req.Cidade,
                Bairro = req.Bairro,
                Estado = req.Estado,
                CEP = req.CEP,
                QtdQuartos = req.QtdQuartos,
                Piscina = req.Piscina,
                Tipo = req.Tipo,
                AnoConstrucao = req.AnoConstrucao,
                AnoReferencia = req.AnoReferencia,
                AreaM2 = req.AreaM2,
                VagasGaragem = req.VagasGaragem,
                CondominioMensal = req.CondominioMensal,
                DistCentroKm = req.DistCentroKm,
                ProxParque = req.ProxParque
            };

            var pred = _pool.Predict(modelName: "ImovelModel", example: input);
            var culturaBR = new CultureInfo("pt-BR");

            return Ok(new PredictResponse
            {
                PrecoPrevisto = (decimal)pred.Score,
                PrecoPrevistoFormatado = pred.Score.ToString("C", culturaBR)
            });
        }
    }
}