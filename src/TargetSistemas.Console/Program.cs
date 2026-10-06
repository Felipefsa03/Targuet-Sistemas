using System.Globalization;
using System.Text.Json;
using TargetSistemas.Core.Comissoes;
using TargetSistemas.Core.Estoque;
using TargetSistemas.Core.Juros;

namespace TargetSistemas.App;

internal static class Program
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static int Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = PtBr;
        CultureInfo.DefaultThreadCurrentUICulture = PtBr;

        try
        {
            var raiz = EncontrarRaizRepositorio();
            var exercicio = ObterArgumento(args, "--exercicio");

            if (!string.IsNullOrWhiteSpace(exercicio))
                return ExecutarPorArgumentos(exercicio, args, raiz);

            return ExecutarMenuPrincipal(raiz);
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Erro ao ler JSON: {ex.Message}");
            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erro: {ex.Message}");
            return 1;
        }
    }

    private static int ExecutarMenuPrincipal(string raiz)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine(" TESTE TÉCNICO - TARGET SISTEMAS");
            Console.WriteLine("========================================");
            Console.WriteLine("1 - Comissão de vendedores");
            Console.WriteLine("2 - Movimentação de estoque");
            Console.WriteLine("3 - Cálculo de juros");
            Console.WriteLine("0 - Sair");
            Console.Write("Escolha uma opção: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ExecutarComissoes(raiz);
                    break;
                case "2":
                    ExecutarEstoqueInterativo(raiz);
                    break;
                case "3":
                    ExecutarJurosInterativo();
                    break;
                case "0":
                    return 0;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
    }

    private static int ExecutarPorArgumentos(string exercicio, string[] args, string raiz)
    {
        switch (exercicio)
        {
            case "1":
                ExecutarComissoes(raiz);
                return 0;
            case "2":
            {
                var arquivo = ObterArgumento(args, "--arquivo");
                if (string.IsNullOrWhiteSpace(arquivo))
                    ExecutarEstoqueInterativo(raiz);
                else
                    ExecutarEstoqueArquivo(raiz, ResolverCaminho(raiz, arquivo));
                return 0;
            }
            case "3":
                ExecutarJurosPorArgumentos(args);
                return 0;
            default:
                throw new ArgumentException("--exercicio deve ser 1, 2 ou 3.");
        }
    }

    private static void ExecutarComissoes(string raiz)
    {
        var arquivo = JsonFileStore.Ler<VendasArquivo>(Path.Combine(raiz, "data", "vendas.json"));
        var vendas = arquivo.Vendas ?? throw new JsonException("A propriedade 'vendas' é obrigatória.");

        Console.WriteLine();
        Console.WriteLine("EXERCÍCIO 1 - COMISSÕES");
        Console.WriteLine(new string('-', 90));
        Console.WriteLine($"{"Vendedor",-22} {"Venda",14} {"Taxa",8} {"Comissão",14}");
        Console.WriteLine(new string('-', 90));

        foreach (var venda in vendas)
        {
            var percentual = ComissaoService.ObterPercentual(venda.Valor);
            var comissao = ComissaoService.Calcular(venda.Valor);
            Console.WriteLine(
                $"{venda.Vendedor,-22} {venda.Valor,14:C2} {percentual,8:P0} {comissao,14:C2}");
        }

        Console.WriteLine();
        Console.WriteLine("RESUMO POR VENDEDOR");
        Console.WriteLine(new string('-', 90));

        var resumos = ComissaoService.ResumirPorVendedor(vendas);
        foreach (var resumo in resumos)
        {
            Console.WriteLine(
                $"{resumo.Vendedor,-22} | vendas: {resumo.QuantidadeVendas,2} | total: {resumo.TotalVendido,12:C2} | comissão: {resumo.ComissaoTotal,10:C2}");
        }

        Console.WriteLine(new string('-', 90));
        Console.WriteLine($"Total vendido pela equipe: {vendas.Sum(v => v.Valor):C2}");
        Console.WriteLine($"Comissão total da equipe: {vendas.Sum(v => ComissaoService.Calcular(v.Valor)):C2}");
    }

    private static void ExecutarEstoqueInterativo(string raiz)
    {
        var service = CriarEstoqueService(raiz);
        var historicoPath = Path.Combine(raiz, "movimentacoes-realizadas.json");

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("EXERCÍCIO 2 - ESTOQUE");
            Console.WriteLine("1 - Entrada");
            Console.WriteLine("2 - Saída");
            Console.WriteLine("3 - Listar estoque");
            Console.WriteLine("4 - Listar movimentações");
            Console.WriteLine("0 - Voltar");
            Console.Write("Escolha: ");

            var opcao = Console.ReadLine()?.Trim();
            if (opcao == "0")
                return;

            if (opcao == "3")
            {
                ImprimirEstoque(service);
                continue;
            }

            if (opcao == "4")
            {
                ImprimirMovimentacoes(service);
                continue;
            }

            if (opcao is not ("1" or "2"))
            {
                Console.WriteLine("Opção inválida.");
                continue;
            }

            try
            {
                ImprimirEstoque(service);
                var codigo = LerInteiroPositivo("Código do produto: ");
                var quantidade = LerInteiroPositivo("Quantidade: ");
                Console.Write("Descrição da movimentação: ");
                var descricao = Console.ReadLine() ?? string.Empty;
                var tipo = opcao == "1" ? TipoMovimentacao.Entrada : TipoMovimentacao.Saida;

                var movimento = service.Registrar(codigo, tipo, quantidade, descricao);
                SalvarHistorico(historicoPath, service);

                Console.WriteLine($"Movimentação #{movimento.Id} registrada com sucesso.");
                Console.WriteLine($"Saldo final do produto {movimento.CodigoProduto}: {movimento.SaldoAposMovimentacao} unidade(s).");
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
            {
                Console.WriteLine($"Movimentação recusada: {ex.Message}");
            }
        }
    }

    private static void ExecutarEstoqueArquivo(string raiz, string arquivoMovimentacoes)
    {
        var service = CriarEstoqueService(raiz);
        var lote = JsonFileStore.Ler<MovimentacoesArquivo>(arquivoMovimentacoes);
        var movimentacoes = lote.Movimentacoes ?? throw new JsonException("A propriedade 'movimentacoes' é obrigatória.");
        var historicoPath = Path.Combine(raiz, "movimentacoes-realizadas.json");

        Console.WriteLine("EXERCÍCIO 2 - PROCESSAMENTO EM LOTE");
        foreach (var item in movimentacoes)
        {
            try
            {
                var movimento = service.Registrar(
                    item.CodigoProduto,
                    item.Tipo,
                    item.Quantidade,
                    item.Descricao);

                SalvarHistorico(historicoPath, service);
                Console.WriteLine(
                    $"OK #{movimento.Id}: produto {movimento.CodigoProduto}, {movimento.Tipo}, {movimento.Quantidade} un. -> saldo {movimento.SaldoAposMovimentacao}");
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
            {
                Console.WriteLine(
                    $"ERRO: produto {item.CodigoProduto}, {item.Tipo}, {item.Quantidade} un. -> {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("ESTOQUE FINAL CONSOLIDADO");
        ImprimirEstoque(service);
    }

    private static EstoqueService CriarEstoqueService(string raiz)
    {
        var dados = JsonFileStore.Ler<EstoqueArquivo>(Path.Combine(raiz, "data", "estoque.json"));
        var produtos = dados.Estoque ?? throw new JsonException("A propriedade 'estoque' é obrigatória.");
        var service = new EstoqueService(produtos);

        var historicoPath = Path.Combine(raiz, "movimentacoes-realizadas.json");
        if (!File.Exists(historicoPath))
            return service;

        var historico = JsonFileStore.Ler<HistoricoMovimentacoes>(historicoPath);
        foreach (var movimento in (historico.Movimentacoes ?? []).OrderBy(m => m.Id))
        {
            var recriado = service.Registrar(
                movimento.CodigoProduto,
                movimento.Tipo,
                movimento.Quantidade,
                movimento.Descricao,
                movimento.DataHora);

            if (recriado.Id != movimento.Id || recriado.SaldoAposMovimentacao != movimento.SaldoAposMovimentacao)
                throw new InvalidOperationException("O histórico de movimentações está inconsistente com o estoque base.");
        }

        return service;
    }

    private static void SalvarHistorico(string caminho, EstoqueService service) =>
        JsonFileStore.Salvar(caminho, new HistoricoMovimentacoes(service.ListarMovimentacoes().ToList()));

    private static void ImprimirEstoque(EstoqueService service)
    {
        Console.WriteLine();
        Console.WriteLine($"{"Código",8} | {"Produto",32} | {"Saldo",8}");
        Console.WriteLine(new string('-', 56));
        foreach (var produto in service.ListarProdutos())
            Console.WriteLine($"{produto.CodigoProduto,8} | {produto.DescricaoProduto,-32} | {produto.Estoque,8}");
    }

    private static void ImprimirMovimentacoes(EstoqueService service)
    {
        var movimentacoes = service.ListarMovimentacoes();
        if (movimentacoes.Count == 0)
        {
            Console.WriteLine("Nenhuma movimentação registrada.");
            return;
        }

        foreach (var m in movimentacoes)
        {
            Console.WriteLine(
                $"#{m.Id} | {m.DataHora:dd/MM/yyyy HH:mm:ss} | Produto {m.CodigoProduto} | {m.Tipo} {m.Quantidade} | Saldo {m.SaldoAposMovimentacao} | {m.Descricao}");
        }
    }

    private static void ExecutarJurosInterativo()
    {
        Console.WriteLine();
        Console.WriteLine("EXERCÍCIO 3 - JUROS");
        var valor = LerDecimalPositivo("Valor: ");
        var vencimento = LerData("Data de vencimento (dd/MM/yyyy ou yyyy-MM-dd): ");
        ImprimirResultadoJuros(JurosService.Calcular(valor, vencimento, DateOnly.FromDateTime(DateTime.Today)));
    }

    private static void ExecutarJurosPorArgumentos(string[] args)
    {
        var valorTexto = ObterArgumento(args, "--valor")
                         ?? throw new ArgumentException("Informe --valor para o exercício 3.");
        var vencimentoTexto = ObterArgumento(args, "--vencimento")
                              ?? throw new ArgumentException("Informe --vencimento para o exercício 3.");

        if (!TentarParseDecimal(valorTexto, out var valor) || valor <= 0m)
            throw new ArgumentException("Valor inválido.");
        if (!TentarParseData(vencimentoTexto, out var vencimento))
            throw new ArgumentException("Data de vencimento inválida. Use dd/MM/yyyy ou yyyy-MM-dd.");

        ImprimirResultadoJuros(JurosService.Calcular(valor, vencimento, DateOnly.FromDateTime(DateTime.Today)));
    }

    private static void ImprimirResultadoJuros(ResultadoJuros resultado)
    {
        Console.WriteLine($"Valor original: {resultado.ValorOriginal:C2}");
        Console.WriteLine($"Vencimento: {resultado.Vencimento:dd/MM/yyyy}");
        Console.WriteLine($"Data do cálculo: {resultado.DataCalculo:dd/MM/yyyy}");
        Console.WriteLine($"Dias de atraso: {resultado.DiasAtraso}");
        Console.WriteLine($"Taxa diária: {resultado.TaxaDiaria:P1}");
        Console.WriteLine($"Juros: {resultado.Juros:C2}");
        Console.WriteLine($"Total: {resultado.Total:C2}");
    }

    private static int LerInteiroPositivo(string mensagem)
    {
        while (true)
        {
            Console.Write(mensagem);
            if (int.TryParse(Console.ReadLine(), out var valor) && valor > 0)
                return valor;
            Console.WriteLine("Informe um número inteiro maior que zero.");
        }
    }

    private static decimal LerDecimalPositivo(string mensagem)
    {
        while (true)
        {
            Console.Write(mensagem);
            if (TentarParseDecimal(Console.ReadLine(), out var valor) && valor > 0m)
                return valor;
            Console.WriteLine("Informe um valor monetário maior que zero.");
        }
    }

    private static DateOnly LerData(string mensagem)
    {
        while (true)
        {
            Console.Write(mensagem);
            if (TentarParseData(Console.ReadLine(), out var data))
                return data;
            Console.WriteLine("Data inválida. Use dd/MM/yyyy ou yyyy-MM-dd.");
        }
    }

    private static bool TentarParseDecimal(string? texto, out decimal valor)
    {
        if (decimal.TryParse(texto, NumberStyles.Number, PtBr, out valor))
            return true;

        return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
    }

    private static bool TentarParseData(string? texto, out DateOnly data)
    {
        var formatos = new[] { "dd/MM/yyyy", "yyyy-MM-dd" };
        return DateOnly.TryParseExact(texto, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out data);
    }

    private static string? ObterArgumento(string[] args, string nome)
    {
        var indice = Array.FindIndex(args, a => string.Equals(a, nome, StringComparison.OrdinalIgnoreCase));
        return indice >= 0 && indice + 1 < args.Length ? args[indice + 1] : null;
    }

    private static string ResolverCaminho(string raiz, string caminho) =>
        Path.IsPathRooted(caminho) ? caminho : Path.Combine(raiz, caminho);

    private static string EncontrarRaizRepositorio()
    {
        DirectoryInfo? atual = new(Directory.GetCurrentDirectory());
        while (atual is not null)
        {
            if (File.Exists(Path.Combine(atual.FullName, "TargetSistemas.Teste.sln")))
                return atual.FullName;
            atual = atual.Parent;
        }

        atual = new DirectoryInfo(AppContext.BaseDirectory);
        while (atual is not null)
        {
            if (File.Exists(Path.Combine(atual.FullName, "TargetSistemas.Teste.sln")))
                return atual.FullName;
            atual = atual.Parent;
        }

        throw new DirectoryNotFoundException("Não foi possível localizar a raiz do projeto.");
    }
}
