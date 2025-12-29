using Microsoft.ML.Data;

namespace MLImoveisCuritiba.Shared.Models;

public class ModelInput
{
    [LoadColumn(0)]
    [ColumnName(@"Cidade")]
    public string Cidade { get; set; }

    [LoadColumn(1)]
    [ColumnName(@"Bairro")]
    public string Bairro { get; set; }

    [LoadColumn(2)]
    [ColumnName(@"Estado")]
    public string Estado { get; set; }

    [LoadColumn(3)]
    [ColumnName(@"CEP")]
    public string CEP { get; set; }

    [LoadColumn(4)]
    [ColumnName(@"QtdQuartos")]
    public float QtdQuartos { get; set; }

    [LoadColumn(5)]
    [ColumnName(@"Piscina")]
    public string Piscina { get; set; }

    [LoadColumn(6)]
    [ColumnName(@"Tipo")]
    public string Tipo { get; set; }

    [LoadColumn(7)]
    [ColumnName(@"AnoConstrucao")]
    public float AnoConstrucao { get; set; }

    [LoadColumn(8)]
    [ColumnName(@"AnoReferencia")]
    public float AnoReferencia { get; set; }

    [LoadColumn(9)]
    [ColumnName(@"AreaM2")]
    public float AreaM2 { get; set; }

    [LoadColumn(10)]
    [ColumnName(@"VagasGaragem")]
    public float VagasGaragem { get; set; }

    [LoadColumn(11)]
    [ColumnName(@"CondominioMensal")]
    public float CondominioMensal { get; set; }

    [LoadColumn(12)]
    [ColumnName(@"DistCentroKm")]
    public float DistCentroKm { get; set; }

    [LoadColumn(13)]
    [ColumnName(@"ProxParque")]
    public string ProxParque { get; set; }

    [LoadColumn(14)]
    [ColumnName(@"Label")]
    public float PrecoVenda { get; set; }

}