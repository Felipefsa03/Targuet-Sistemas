namespace TargetSistemas.Core.Comissoes;

public sealed record VendedorResumo(
    string Vendedor,
    int QuantidadeVendas,
    decimal TotalVendido,
    decimal ComissaoTotal);
