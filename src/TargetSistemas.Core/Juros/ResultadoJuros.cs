namespace TargetSistemas.Core.Juros;

public sealed record ResultadoJuros(
    decimal ValorOriginal,
    DateOnly Vencimento,
    DateOnly DataCalculo,
    int DiasAtraso,
    decimal TaxaDiaria,
    decimal Juros,
    decimal Total);
