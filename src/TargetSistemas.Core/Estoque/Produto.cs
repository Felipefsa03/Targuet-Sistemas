using System.Text.Json.Serialization;

namespace TargetSistemas.Core.Estoque;

public sealed class Produto
{
    [JsonConstructor]
    public Produto(int codigoProduto, string descricaoProduto, int estoque)
    {
        if (codigoProduto <= 0)
            throw new ArgumentOutOfRangeException(nameof(codigoProduto));
        if (string.IsNullOrWhiteSpace(descricaoProduto))
            throw new ArgumentException("A descrição do produto é obrigatória.", nameof(descricaoProduto));
        if (estoque < 0)
            throw new ArgumentOutOfRangeException(nameof(estoque));

        CodigoProduto = codigoProduto;
        DescricaoProduto = descricaoProduto;
        Estoque = estoque;
    }

    public int CodigoProduto { get; }
    public string DescricaoProduto { get; }
    public int Estoque { get; private set; }

    internal void Adicionar(int quantidade) => Estoque += quantidade;
    internal void Remover(int quantidade) => Estoque -= quantidade;
}
