namespace TargetSistemas.Core.Estoque;

public sealed class EstoqueService
{
    private readonly Dictionary<int, Produto> _produtos;
    private readonly List<Movimentacao> _movimentacoes;
    private long _proximoId;

    public EstoqueService(
        IEnumerable<Produto> produtos,
        IEnumerable<Movimentacao>? movimentacoesExistentes = null)
    {
        ArgumentNullException.ThrowIfNull(produtos);

        var listaProdutos = produtos.ToList();
        if (listaProdutos.Count == 0)
            throw new ArgumentException("É necessário informar ao menos um produto.", nameof(produtos));

        if (listaProdutos.Select(p => p.CodigoProduto).Distinct().Count() != listaProdutos.Count)
            throw new ArgumentException("Existem códigos de produto duplicados.", nameof(produtos));

        _produtos = listaProdutos.ToDictionary(p => p.CodigoProduto);
        _movimentacoes = movimentacoesExistentes?.ToList() ?? [];

        if (_movimentacoes.Any(m => m.Id <= 0))
            throw new ArgumentException("Todos os IDs de movimentação existentes devem ser positivos.", nameof(movimentacoesExistentes));

        if (_movimentacoes.Select(m => m.Id).Distinct().Count() != _movimentacoes.Count)
            throw new ArgumentException("Existem IDs de movimentação duplicados.", nameof(movimentacoesExistentes));

        _proximoId = _movimentacoes.Count == 0 ? 1 : _movimentacoes.Max(m => m.Id) + 1;
    }

    public IReadOnlyList<Produto> ListarProdutos() =>
        _produtos.Values.OrderBy(p => p.CodigoProduto).ToList();

    public IReadOnlyList<Movimentacao> ListarMovimentacoes() =>
        _movimentacoes.OrderBy(m => m.Id).ToList();

    public Movimentacao Registrar(
        int codigoProduto,
        TipoMovimentacao tipo,
        int quantidade,
        string descricao,
        DateTimeOffset? dataHora = null)
    {
        if (!_produtos.TryGetValue(codigoProduto, out var produto))
            throw new KeyNotFoundException($"Produto {codigoProduto} não encontrado.");

        if (!Enum.IsDefined(typeof(TipoMovimentacao), tipo))
            throw new ArgumentOutOfRangeException(nameof(tipo), "Tipo de movimentação inválido.");

        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da movimentação é obrigatória.", nameof(descricao));

        if (tipo == TipoMovimentacao.Saida && quantidade > produto.Estoque)
            throw new InvalidOperationException(
                $"Estoque insuficiente para o produto {produto.CodigoProduto}. Saldo disponível: {produto.Estoque}.");

        if (tipo == TipoMovimentacao.Entrada)
            produto.Adicionar(quantidade);
        else
            produto.Remover(quantidade);

        var movimentacao = new Movimentacao(
            _proximoId++,
            codigoProduto,
            tipo,
            descricao.Trim(),
            quantidade,
            dataHora ?? DateTimeOffset.Now,
            produto.Estoque);

        _movimentacoes.Add(movimentacao);
        return movimentacao;
    }
}
