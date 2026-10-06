namespace TargetSistemas.Core.Comissoes;

public static class ComissaoService
{
    public static decimal ObterPercentual(decimal valor)
    {
        if (valor < 0m)
            throw new ArgumentOutOfRangeException(nameof(valor), "O valor da venda não pode ser negativo.");

        if (valor < 100m)
            return 0m;

        if (valor < 500m)
            return 0.01m;

        return 0.05m;
    }

    public static decimal Calcular(decimal valor)
    {
        var percentual = ObterPercentual(valor);
        return Math.Round(valor * percentual, 2, MidpointRounding.AwayFromZero);
    }

    public static IReadOnlyList<VendedorResumo> ResumirPorVendedor(IEnumerable<Venda> vendas)
    {
        ArgumentNullException.ThrowIfNull(vendas);

        return vendas
            .GroupBy(v => v.Vendedor)
            .Select(grupo => new VendedorResumo(
                grupo.Key,
                grupo.Count(),
                grupo.Sum(v => v.Valor),
                grupo.Sum(v => Calcular(v.Valor))))
            .OrderBy(r => r.Vendedor)
            .ToList();
    }
}
