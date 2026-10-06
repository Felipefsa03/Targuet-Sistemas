namespace TargetSistemas.Core.Juros;

public static class JurosService
{
    public const decimal TaxaDiaria = 0.025m;

    public static ResultadoJuros Calcular(decimal valor, DateOnly vencimento, DateOnly hoje)
    {
        if (valor <= 0m)
            throw new ArgumentOutOfRangeException(nameof(valor), "O valor deve ser maior que zero.");

        var diasAtraso = Math.Max(0, hoje.DayNumber - vencimento.DayNumber);
        var juros = Math.Round(valor * TaxaDiaria * diasAtraso, 2, MidpointRounding.AwayFromZero);
        var total = Math.Round(valor + juros, 2, MidpointRounding.AwayFromZero);

        return new ResultadoJuros(
            valor,
            vencimento,
            hoje,
            diasAtraso,
            TaxaDiaria,
            juros,
            total);
    }
}
