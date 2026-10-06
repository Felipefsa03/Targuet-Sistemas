using TargetSistemas.Core.Estoque;

namespace TargetSistemas.Tests.Estoque;

public sealed class EstoqueServiceTests
{
    [Fact]
    public void Entrada_DeveAumentarSaldoERetornarSaldoFinal()
    {
        var service = CriarService();

        var movimento = service.Registrar(101, TipoMovimentacao.Entrada, 50, "Reposição");

        Assert.Equal(200, movimento.SaldoAposMovimentacao);
        Assert.Equal(200, service.ListarProdutos().Single(p => p.CodigoProduto == 101).Estoque);
    }

    [Fact]
    public void Saida_DeveDiminuirSaldo()
    {
        var service = CriarService();

        var movimento = service.Registrar(101, TipoMovimentacao.Saida, 25, "Venda");

        Assert.Equal(125, movimento.SaldoAposMovimentacao);
    }

    [Fact]
    public void Id_DeveSerSequencialEUnico()
    {
        var service = CriarService();

        var primeiro = service.Registrar(101, TipoMovimentacao.Entrada, 1, "Entrada 1");
        var segundo = service.Registrar(101, TipoMovimentacao.Saida, 1, "Saída 1");

        Assert.Equal(1, primeiro.Id);
        Assert.Equal(2, segundo.Id);
        Assert.NotEqual(primeiro.Id, segundo.Id);
    }

    [Fact]
    public void SaidaMaiorQueSaldo_DeveFalharESaldoPermanecerInalterado()
    {
        var service = CriarService();

        Assert.Throws<InvalidOperationException>(() =>
            service.Registrar(102, TipoMovimentacao.Saida, 76, "Saída inválida"));

        Assert.Equal(75, service.ListarProdutos().Single(p => p.CodigoProduto == 102).Estoque);
        Assert.Empty(service.ListarMovimentacoes());
    }

    [Fact]
    public void ProdutoInexistente_DeveFalhar()
    {
        var service = CriarService();
        Assert.Throws<KeyNotFoundException>(() =>
            service.Registrar(999, TipoMovimentacao.Entrada, 1, "Teste"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void QuantidadeNaoPositiva_DeveFalhar(int quantidade)
    {
        var service = CriarService();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.Registrar(101, TipoMovimentacao.Entrada, quantidade, "Teste"));
    }

    [Fact]
    public void SequenciaDeMovimentos_DeveProduzirSaldoFinalCorreto()
    {
        var service = CriarService();

        service.Registrar(101, TipoMovimentacao.Entrada, 50, "1");
        service.Registrar(101, TipoMovimentacao.Saida, 20, "2");
        service.Registrar(101, TipoMovimentacao.Entrada, 15, "3");
        service.Registrar(101, TipoMovimentacao.Saida, 10, "4");
        var ultimo = service.Registrar(101, TipoMovimentacao.Saida, 5, "5");

        Assert.Equal(180, ultimo.SaldoAposMovimentacao);
        Assert.Equal(5, service.ListarMovimentacoes().Count);
    }

    [Fact]
    public void HistoricoExistente_DeveContinuarSequenciaDeIds()
    {
        var historico = new[]
        {
            new Movimentacao(1, 101, TipoMovimentacao.Entrada, "Anterior", 10, DateTimeOffset.UtcNow, 160),
            new Movimentacao(2, 101, TipoMovimentacao.Saida, "Anterior", 5, DateTimeOffset.UtcNow, 155)
        };
        var service = new EstoqueService(
            [new Produto(101, "Caneta Azul", 155)],
            historico);

        var novo = service.Registrar(101, TipoMovimentacao.Entrada, 1, "Novo");

        Assert.Equal(3, novo.Id);
    }

    private static EstoqueService CriarService() => new(
    [
        new Produto(101, "Caneta Azul", 150),
        new Produto(102, "Caderno Universitário", 75)
    ]);
}
