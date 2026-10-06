using TargetSistemas.Core.Comissoes;

namespace TargetSistemas.Tests.Comissoes;

public sealed class ComissaoServiceTests
{
    [Theory]
    [InlineData("0.00", "0.00")]
    [InlineData("99.99", "0.00")]
    [InlineData("100.00", "1.00")]
    [InlineData("499.99", "5.00")]
    [InlineData("500.00", "25.00")]
    [InlineData("1200.50", "60.03")]
    public void Calcular_DeveRespeitarFaixasEArredondamento(string valorTexto, string esperadoTexto)
    {
        var valor = decimal.Parse(valorTexto, System.Globalization.CultureInfo.InvariantCulture);
        var esperado = decimal.Parse(esperadoTexto, System.Globalization.CultureInfo.InvariantCulture);

        var resultado = ComissaoService.Calcular(valor);

        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void Calcular_ValorNegativo_DeveFalhar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ComissaoService.Calcular(-0.01m));
    }

    [Fact]
    public void ResumirPorVendedor_ListaVazia_DeveRetornarVazio()
    {
        var resultado = ComissaoService.ResumirPorVendedor([]);
        Assert.Empty(resultado);
    }

    [Fact]
    public void ResumirPorVendedor_DeveSomarComissaoArredondadaPorVenda()
    {
        var vendas = new[]
        {
            new Venda("João Silva", 1200.50m),
            new Venda("João Silva", 950.75m),
            new Venda("João Silva", 1800.00m),
            new Venda("João Silva", 1400.30m),
            new Venda("João Silva", 1100.90m),
            new Venda("João Silva", 1550.00m),
            new Venda("João Silva", 1700.80m),
            new Venda("João Silva", 250.30m),
            new Venda("João Silva", 480.75m),
            new Venda("João Silva", 320.40m)
        };

        var resumo = Assert.Single(ComissaoService.ResumirPorVendedor(vendas));

        Assert.Equal(10, resumo.QuantidadeVendas);
        Assert.Equal(10754.70m, resumo.TotalVendido);
        Assert.Equal(495.69m, resumo.ComissaoTotal);
    }

    [Fact]
    public void DadosDoEnunciado_DevemGerarTotalGeralEsperado()
    {
        var vendas = CriarVendasDoEnunciado();

        Assert.Equal(37321.30m, vendas.Sum(v => v.Valor));
        Assert.Equal(1746.02m, vendas.Sum(v => ComissaoService.Calcular(v.Valor)));

        var resumos = ComissaoService.ResumirPorVendedor(vendas).ToDictionary(r => r.Vendedor);
        Assert.Equal(495.69m, resumos["João Silva"].ComissaoTotal);
        Assert.Equal(465.96m, resumos["Maria Souza"].ComissaoTotal);
        Assert.Equal(379.38m, resumos["Carlos Oliveira"].ComissaoTotal);
        Assert.Equal(404.99m, resumos["Ana Lima"].ComissaoTotal);
    }

    private static List<Venda> CriarVendasDoEnunciado() =>
    [
        new("João Silva", 1200.50m), new("João Silva", 950.75m), new("João Silva", 1800.00m),
        new("João Silva", 1400.30m), new("João Silva", 1100.90m), new("João Silva", 1550.00m),
        new("João Silva", 1700.80m), new("João Silva", 250.30m), new("João Silva", 480.75m),
        new("João Silva", 320.40m),
        new("Maria Souza", 2100.40m), new("Maria Souza", 1350.60m), new("Maria Souza", 950.20m),
        new("Maria Souza", 1600.75m), new("Maria Souza", 1750.00m), new("Maria Souza", 1450.90m),
        new("Maria Souza", 400.50m), new("Maria Souza", 180.20m), new("Maria Souza", 90.75m),
        new("Carlos Oliveira", 800.50m), new("Carlos Oliveira", 1200.00m), new("Carlos Oliveira", 1950.30m),
        new("Carlos Oliveira", 1750.80m), new("Carlos Oliveira", 1300.60m), new("Carlos Oliveira", 300.40m),
        new("Carlos Oliveira", 500.00m), new("Carlos Oliveira", 125.75m),
        new("Ana Lima", 1000.00m), new("Ana Lima", 1100.50m), new("Ana Lima", 1250.75m),
        new("Ana Lima", 1400.20m), new("Ana Lima", 1550.90m), new("Ana Lima", 1650.00m),
        new("Ana Lima", 75.30m), new("Ana Lima", 420.90m), new("Ana Lima", 315.40m)
    ];
}
