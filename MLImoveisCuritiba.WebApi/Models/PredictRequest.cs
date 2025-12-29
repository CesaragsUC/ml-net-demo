namespace MLImoveisCuritiba.WebApi.Models;

public class PredictRequest
{
    public string Cidade { get; set; } = "";
    public string Bairro { get; set; } = "";
    public string Estado { get; set; } = "";
    public string CEP { get; set; } = "";

    public float QtdQuartos { get; set; }
    public string Piscina { get; set; } = "";  // SIM/NAO
    public string Tipo { get; set; } = "";     // casa/apartamento

    public float AnoConstrucao { get; set; }
    public float AnoReferencia { get; set; }

    public float AreaM2 { get; set; }
    public float VagasGaragem { get; set; }
    public float CondominioMensal { get; set; }
    public float DistCentroKm { get; set; }

    public string ProxParque { get; set; } = ""; // SIM/NAO
}
