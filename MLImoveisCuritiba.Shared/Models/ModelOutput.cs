using Microsoft.ML.Data;

namespace MLImoveisCuritiba.Shared.Models;

/// <summary>
/// model output class for ImovelVenda.
/// </summary>
public class ModelOutput
{

    [ColumnName(@"Features")]
    public float[] Features { get; set; }

    [ColumnName(@"Score")]
    public float Score { get; set; }

}
