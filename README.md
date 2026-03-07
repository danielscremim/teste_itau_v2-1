# Sistema de Compra Programada de Ações — Itaú Corretora

Implementação do desafio técnico de **Compra Programada de Ações** com .NET 9, MySQL e Apache Kafka.

---

## Índice

1. [Visão Geral](#visão-geral)
2. [Tecnologias](#tecnologias)
3. [Estrutura da Solução](#estrutura-da-solução)
4. [Pré-requisitos](#pré-requisitos)
5. [Como rodar](#como-rodar)
6. [Testes Unitários](#testes-unitários)
7. [Principais Regras de Negócio](#principais-regras-de-negócio)

---

## Visão Geral

O sistema permite que clientes da Itaú Corretora reservem um valor mensal fixo para comprar automaticamente uma **cesta de 5 ações** (Top Five), definida pelos administradores.

No uso do sistema:
- Clientes fazem adesão informando nome, CPF, e-mail e valor mensal
- Administradores cadastram a cesta Top Five (5 ativos com percentuais que somam 100%)
- No **5º dia útil do mês**, o motor de compras executa automaticamente
- O sistema calcula quantas ações comprar para cada cliente (regra: TRUNCAR, nunca arredondar)
- As compras são distribuídas entre os clientes proporcionalmente
- O IR dedo-duro (0,005%) é publicado no Kafka para recolhimento automático

---

## Tecnologias

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET | 9.0 | Framework principal |
| ASP.NET Core | 9.0 | API REST |
| Entity Framework Core | 9.0 | ORM para MySQL |
| Pomelo MySQL | 9.0 | Driver MySQL para EF Core |
| MySQL | 8.0 | Banco de dados relacional |
| Apache Kafka | 7.5 | Mensageria para IR dedo-duro |
| FluentValidation | 11.x | Validação de requests |
| xUnit | 2.9 | Framework de testes |
| Moq | 4.20 | Mock para testes |
| FluentAssertions | 8.8 | Assertions legíveis |
| Swagger / OpenAPI | — | Documentação da API |

---

## Estrutura da Solução

```
ComprasProgramadas.sln
│
├── ComprasProgramadas.Domain        # Entidades, regras de negócio, interfaces
│   ├── Entities/                    # Cliente, CestaTopFive, CustodiaFilhote, etc.
│   ├── Exceptions/                  # DomainException
│   ├── Interfaces/                  # IUnitOfWork, ICotahistParser, IKafkaPublisher
│   └── Interfaces/Repositories/     # Contratos dos repositórios
│
├── ComprasProgramadas.Infrastructure # Implementações de infra
│   ├── Data/                        # AppDbContext + configurações EF
│   ├── Repositories/                # Implementações dos repositórios
│   ├── Migrations/                  # EF Core migrations
│   ├── B3/                          # Parser do arquivo COTAHIST da B3
│   └── Kafka/                       # KafkaPublisher
│
├── ComprasProgramadas.Application   # Use Cases, DTOs, Validators
│   ├── UseCases/Clientes/           # Aderir, Sair, AlterarValor, ConsultarCarteira
│   ├── UseCases/Admin/              # CadastrarCesta, ImportarCotacoes, ExecutarMotor
│   ├── DTOs/Requests/               # Objetos de entrada da API
│   ├── DTOs/Responses/              # Objetos de retorno da API
│   └── Validators/                  # FluentValidation validators
│
├── ComprasProgramadas.API           # Controllers + Program.cs
│   ├── Controllers/ClienteController.cs
│   └── Controllers/AdminController.cs
│
└── ComprasProgramadas.Tests         # Testes unitários
    ├── Domain/                      # Testes das entidades de domínio
    ├── Validators/                  # Testes dos validators
    └── UseCases/                    # Testes dos Use Cases (com Moq)
```

---

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [dotnet-ef CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

---

## Como rodar

### 1. Subir os containers (MySQL + Kafka)

```bash
# MySQL 8
docker run -d --name compras_mysql \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=compras_programadas \
  -p 3306:3306 \
  mysql:8.0

# Zookeeper (requisito do Kafka)
docker run -d --name compras_zookeeper \
  -e ZOOKEEPER_CLIENT_PORT=2181 \
  -p 2181:2181 \
  confluentinc/cp-zookeeper:7.5.0

# Kafka
docker run -d --name compras_kafka \
  -e KAFKA_BROKER_ID=1 \
  -e KAFKA_ZOOKEEPER_CONNECT=host.docker.internal:2181 \
  -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 \
  -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 \
  -p 9092:9092 \
  confluentinc/cp-kafka:7.5.0
```

### 2. Criar as tabelas no banco

```bash
cd ComprasProgramadas.API
dotnet ef database update
```

### 3. Rodar a API

```bash
dotnet run --project ComprasProgramadas.API
# API disponível em: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

---

## Testes Unitários

### Rodar os testes

```bash
dotnet test ComprasProgramadas.Tests
```

### Rodar com relatório de cobertura

```bash
dotnet test ComprasProgramadas.Tests --collect:"XPlat Code Coverage"
```

### Resultado dos testes

| Métrica | Valor |
|---|---|
| Total de testes | 96 |
| Aprovados | 96 |
| Com falhas | 0 |
| Cobertura — Domain | **85.9%** |
| Cobertura — Application | **59.3%** |
| **Cobertura total** | **72.1%** ✅ |

### Organização dos testes

```
ComprasProgramadas.Tests/
  Domain/
    ClienteTests.cs             # Criar, Desativar, AlterarValorMensal
    CestaTopFiveTests.cs        # Criar (5 itens, soma=100%), Desativar
    CustodiaFilhoteTests.cs     # RegistrarCompra (PM), RegistrarVenda (PM não muda)
    CustodiaMasterTests.cs      # AdicionarResiduo, Descontar
    ItemOrdemCompraTests.cs     # TRUNCAR, lote/fracionário, saldo Master
    DistribuicaoTests.cs        # IR dedo-duro 0.005%
    OrdemCompraTests.cs         # Criar, MarcarExecutada, MarcarErro
    ContaGraficaTests.cs        # CriarFilhote, CriarMaster
    RebalanceamentoTests.cs     # CriarPorMudancaCesta, MarcarExecutado
    RebalanceamentoClienteTests.cs  # RegistrarResultado, MarcarErro
    VendaMesTests.cs            # Registrar, cálculo de lucro
  Validators/
    ValidatorsTests.cs          # AdesaoValidator, CadastrarCestaValidator, etc.
  UseCases/
    AderirAoProdutoTests.cs     # CPF novo vs. duplicado, numeração de conta
    SairDoProdutoTests.cs       # Desativar cliente, cliente inexistente
    AlterarValorMensalTests.cs  # Histórico, cliente inativo
    ConsultarCarteiraTests.cs   # P/L, rentabilidade, carteira vazia
    CadastrarCestaTopFiveTests.cs  # Desativar cesta anterior, criar rebalanceamento
    ImportarCotacoesTests.cs    # Arquivo não encontrado, importação vazia
    ExecutarMotorCompraTests.cs # Sem cesta ativa, sem clientes
```

### Padrão dos testes

Todos os testes seguem o padrão **AAA (Arrange-Act-Assert)**:

```csharp
[Fact(DisplayName = "Descrição clara do cenário")]
public void NomeMetodo_Contexto_ResultadoEsperado()
{
    // Arrange — prepara o estado inicial
    var cliente = Cliente.Criar("João", "12345678901", "joao@email.com", 500m);

    // Act — executa a ação sendo testada
    cliente.Desativar();

    // Assert — verifica o resultado esperado
    cliente.Ativo.Should().BeFalse();
}
```

Para Use Cases que dependem de banco de dados, usamos **Moq** para criar dublês:

```csharp
var repoMock = new Mock<IClienteRepository>();
repoMock.Setup(r => r.ObterPorCpfAsync("12345678901")).ReturnsAsync((Cliente?)null);
```

---

## Endpoints da API

### Cliente

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/clientes/adesao` | Adesão ao produto |
| `DELETE` | `/api/clientes/{id}/saida` | Saída do produto |
| `PATCH` | `/api/clientes/{id}/valor-mensal` | Alterar valor mensal |
| `GET` | `/api/clientes/{id}/carteira` | Consultar carteira (resumo) |
| `GET` | `/api/clientes/{id}/rentabilidade` | Rentabilidade detalhada: P/L por ativo, composição vs cesta, histórico mensal, IR acumulado |

### Admin

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/admin/cesta` | Cadastrar nova cesta Top Five |
| `GET` | `/api/admin/cesta` | Visualizar cesta ativa |
| `GET` | `/api/admin/cesta/historico` | Histórico de cestas |
| `POST` | `/api/admin/cotacoes/importar` | Importar arquivo COTAHIST da B3 |
| `POST` | `/api/admin/motor-compra` | Executar motor de compras (manual) |
| `POST` | `/api/admin/rebalanceamento` | Executar rebalanceamentos pendentes |

---

## Principais Regras de Negócio

| Regra | Descrição |
|---|---|
| RN-014 | A cesta deve ter **exatamente 5 ativos** |
| RN-015 | A soma dos percentuais deve ser **exatamente 100%** |
| RN-020/021 | Motor executa nos dias 5, 15, 25 ou próximo dia útil (auto-agendado via `MotorCompraSchedulerService`) |
| RN-028 | Número de ações = `TRUNCAR(ValorAlvo / Cotacao)` — nunca arredonda |
| RN-031/032 | Compras acima de 100 ações: separar **lote padrão** (múltiplos de 100) + **fracionário** |
| RN-033 | Ticker fracionário = ticker + "F" (ex: PETR4 → PETR4F) |
| RN-042 | Preço Médio ponderado ao comprar: `(QtdAnt * PM + QtdNova * Preco) / (QtdAnt + QtdNova)` |
| RN-043 | **PM nunca muda na venda** — apenas na compra |
| RN-045/049 | Mudança de cesta dispara **rebalanceamento automático** para todos os clientes |
| RN-053 | IR dedo-duro = `0,005%` sobre o valor da operação (alíquota: 0,00005) |
| RN-058 | Se vendas do mês > R$ 20.000: cobra **20% de IR** sobre lucro líquido |

---

## Autor

Daniel Scremim — desafio técnico Itaú Corretora
{
  "itens": [
    { "ticker": "PETR4", "percentual": 30 },
    { "ticker": "VALE3", "percentual": 25 },
    { "ticker": "ITUB4", "percentual": 20 },
    { "ticker": "B3SA3", "percentual": 15 },
    { "ticker": "ABEV3", "percentual": 10 }
  ]
}