using TargetSistemas.Core.Comissoes;
using TargetSistemas.Core.Estoque;

namespace TargetSistemas.App;

internal sealed record VendasArquivo(List<Venda> Vendas);
internal sealed record EstoqueArquivo(List<Produto> Estoque);
internal sealed record MovimentacoesArquivo(List<MovimentacaoSolicitada> Movimentacoes);
internal sealed record HistoricoMovimentacoes(List<Movimentacao> Movimentacoes);
internal sealed record MovimentacaoSolicitada(
    int CodigoProduto,
    TipoMovimentacao Tipo,
    int Quantidade,
    string Descricao);
