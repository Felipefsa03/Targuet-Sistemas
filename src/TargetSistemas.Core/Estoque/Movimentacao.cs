namespace TargetSistemas.Core.Estoque;

public sealed record Movimentacao(
    long Id,
    int CodigoProduto,
    TipoMovimentacao Tipo,
    string Descricao,
    int Quantidade,
    DateTimeOffset DataHora,
    int SaldoAposMovimentacao);
