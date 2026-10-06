# Teste Técnico — Target Sistemas

Solução em **C# / .NET 10** para os três exercícios propostos no processo seletivo de Desenvolvedor/a de Sistemas.

O projeto privilegia código simples, regras de negócio testáveis, uso de `decimal` para valores monetários e leitura dos dados fornecidos por arquivos JSON.

## Comando base

```text
cd TargetSistemas.Teste

dotnet restore
dotnet build
dotnet test

dotnet run --project src/TargetSistemas.Console
```

## Estrutura

```text
TargetSistemas.Teste/
├── TargetSistemas.Teste.sln
├── README.md
├── data/
│   ├── vendas.json
│   ├── estoque.json
│   └── movimentacoes-exemplo.json
├── src/
│   ├── TargetSistemas.Core/
│   │   ├── Comissoes/
│   │   ├── Estoque/
│   │   └── Juros/
│   └── TargetSistemas.Console/
└── tests/
    └── TargetSistemas.Tests/
```

## Pré-requisitos

- .NET SDK 10

Confirme a instalação com:

```bash
dotnet --version
```

## Build e testes

Na raiz do projeto:

```bash
dotnet restore
dotnet build
dotnet test
```

## Executando

### Menu interativo

```bash
dotnet run --project src/TargetSistemas.Console
```

O programa apresenta:

```text
1 - Comissão de vendedores
2 - Movimentação de estoque
3 - Cálculo de juros
0 - Sair
```

Também é possível executar os exercícios diretamente.

### Exercício 1 — Comissões

```bash
dotnet run --project src/TargetSistemas.Console -- --exercicio 1
```

O programa lê `data/vendas.json`, calcula a comissão de cada venda e apresenta um resumo por vendedor.

Regras:

| Valor da venda | Comissão |
|---|---:|
| `< R$ 100,00` | 0% |
| `>= R$ 100,00 e < R$ 500,00` | 1% |
| `>= R$ 500,00` | 5% |

A comissão de **cada venda** é arredondada para duas casas decimais com `MidpointRounding.AwayFromZero`. Depois disso, as comissões são somadas por vendedor.

Resultados esperados para o JSON fornecido:

| Vendedor | Total vendido | Comissão |
|---|---:|---:|
| João Silva | R$ 10.754,70 | R$ 495,69 |
| Maria Souza | R$ 9.874,30 | R$ 465,96 |
| Carlos Oliveira | R$ 7.928,35 | R$ 379,38 |
| Ana Lima | R$ 8.763,95 | R$ 404,99 |
| **Equipe** | **R$ 37.321,30** | **R$ 1.746,02** |

### Exercício 2 — Estoque

Modo interativo:

```bash
dotnet run --project src/TargetSistemas.Console -- --exercicio 2
```

O menu permite:

- registrar entrada;
- registrar saída;
- listar estoque;
- listar movimentações.

Cada movimentação recebe um **ID numérico sequencial e único**. O histórico é salvo em `movimentacoes-realizadas.json`, sem sobrescrever `data/estoque.json`.

Ao iniciar novamente o programa, as movimentações anteriores são reaplicadas ao estoque base. Assim, o saldo e a sequência de identificadores são preservados entre execuções.

Validações principais:

- produto precisa existir;
- quantidade precisa ser maior que zero;
- descrição é obrigatória;
- saída não pode ser maior que o saldo disponível;
- uma operação inválida não altera o estoque.

#### Processamento em lote

```bash
dotnet run --project src/TargetSistemas.Console -- \
  --exercicio 2 \
  --arquivo data/movimentacoes-exemplo.json
```

As movimentações são processadas em ordem. Uma movimentação inválida é informada na saída, mas não interrompe o restante do lote.

Para voltar ao estoque original durante testes locais, basta apagar o arquivo gerado:

```bash
rm movimentacoes-realizadas.json
```

### Exercício 3 — Juros

Modo interativo:

```bash
dotnet run --project src/TargetSistemas.Console -- --exercicio 3
```

Ou por argumentos:

```bash
dotnet run --project src/TargetSistemas.Console -- \
  --exercicio 3 \
  --valor 1000 \
  --vencimento 2026-09-01
```

São aceitos os formatos de data `dd/MM/yyyy` e `yyyy-MM-dd`.

A regra usada é:

```text
diasAtraso = max(0, hoje - vencimento)
juros = valor × 0,025 × diasAtraso
total = valor + juros
```

Se a data de vencimento for hoje ou estiver no futuro, os juros são zero.

## Decisões de implementação

- **`decimal` para dinheiro:** evita os problemas de representação binária de `double` em cálculos monetários.
- **R$ 500,00 recebe 5%:** o enunciado diz “a partir de R$ 500,00”.
- **Arredondamento por venda:** cada comissão é tratada como um fato financeiro individual antes da consolidação.
- **Juros simples:** o enunciado informa 2,5% ao dia, mas não menciona capitalização ou juros sobre juros. Por isso, a taxa é aplicada ao valor original por dia de atraso.
- **Data de cálculo recebida pela regra de negócio:** `JurosService` não depende diretamente de `DateTime.Today`, o que torna os testes determinísticos.
- **Estoque nunca negativo:** saídas sem saldo suficiente são recusadas antes de qualquer alteração.
- **JSON original preservado:** as movimentações são registradas em arquivo próprio.
- **Core sem I/O:** as regras de negócio ficam em `TargetSistemas.Core`; leitura de console e arquivos fica no projeto executável.

## Testes automatizados

Os testes xUnit cobrem, entre outros cenários:

- comissão em `99,99`, `100,00`, `499,99` e `500,00`;
- arredondamento monetário;
- totais do JSON original;
- entrada e saída de estoque;
- IDs sequenciais;
- tentativa de estoque negativo;
- produto inexistente;
- sequência com múltiplas movimentações;
- juros para vencimento atual, futuro, 1 dia e 10 dias de atraso;
- rejeição de valores não positivos.

Execute:

```bash
dotnet test
```

## Tecnologias

- C#
- .NET 10
- `System.Text.Json`
- LINQ
- xUnit
# Targuet-Sistemas
