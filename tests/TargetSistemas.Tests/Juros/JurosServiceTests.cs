using TargetSistemas.Core.Juros;

namespace TargetSistemas.Tests.Juros;

public sealed class JurosServiceTests
{
    private static readonly DateOnly Hoje = new(2026, 10, 6);

    [Fact]
    public void VencimentoHoje_DeveRetornarJurosZero()
    {
        var resultado = JurosService.Calcular(100m, Hoje, Hoje);
        Assert.Equal(0, resultado.DiasAtraso);
        Assert.Equal(0m, resultado.Juros);
        Assert.Equal(100m, resultado.Total);
    }

    [Fact]
    public void VencimentoFuturo_DeveRetornarJurosZero()
    {
        var resultado = JurosService.Calcular(100m, Hoje.AddDays(10), Hoje);
        Assert.Equal(0, resultado.DiasAtraso);
        Assert.Equal(0m, resultado.Juros);
    }

    [Fact]
    public void UmDia_DeveCalcularDoisEMeioPorCento()
    {
        var resultado = JurosService.Calcular(100m, Hoje.AddDays(-1), Hoje);
        Assert.Equal(1, resultado.DiasAtraso);
        Assert.Equal(2.50m, resultado.Juros);
        Assert.Equal(102.50m, resultado.Total);
    }

    [Fact]
    public void DezDias_DeveUsarJurosSimples()
    {
        var resultado = JurosService.Calcular(100m, Hoje.AddDays(-10), Hoje);
        Assert.Equal(10, resultado.DiasAtraso);
        Assert.Equal(25.00m, resultado.Juros);
        Assert.Equal(125.00m, resultado.Total);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void ValorNaoPositivo_DeveFalhar(string valorTexto)
    {
        var valor = decimal.Parse(valorTexto, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Throws<ArgumentOutOfRangeException>(() => JurosService.Calcular(valor, Hoje, Hoje));
    }

    [Fact]
    public void DeveArredondarParaDuasCasas()
    {
        var resultado = JurosService.Calcular(10.01m, Hoje.AddDays(-1), Hoje);
        Assert.Equal(0.25m, resultado.Juros);
        Assert.Equal(10.26m, resultado.Total);
    }
}
