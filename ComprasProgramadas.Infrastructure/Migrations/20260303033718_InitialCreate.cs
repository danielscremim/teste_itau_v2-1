using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComprasProgramadas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cestas_top_five",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Ativa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataAtivacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataDesativacao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CriadoPor = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cestas_top_five", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cpf = table.Column<string>(type: "char(11)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValorMensal = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataAdesao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataSaida = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cotacoes_historicas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Ticker = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataPregao = table.Column<DateOnly>(type: "date", nullable: false),
                    PrecoAbertura = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    PrecoMaximo = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    PrecoMinimo = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    PrecoFechamento = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    PrecoMedioDia = table.Column<decimal>(type: "decimal(15,6)", nullable: true),
                    VolumeNegociado = table.Column<decimal>(type: "decimal(20,2)", nullable: true),
                    ArquivoOrigem = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cotacoes_historicas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "custodia_master",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoMedio = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custodia_master", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "itens_cesta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CestaId = table.Column<long>(type: "bigint", nullable: false),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Percentual = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_cesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_itens_cesta_cestas_top_five_CestaId",
                        column: x => x.CestaId,
                        principalTable: "cestas_top_five",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ordens_compra",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CestaId = table.Column<long>(type: "bigint", nullable: false),
                    DataExecucao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataReferencia = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalConsolidado = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ArquivoCotacao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordens_compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ordens_compra_cestas_top_five_CestaId",
                        column: x => x.CestaId,
                        principalTable: "cestas_top_five",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rebalanceamentos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CestaNovaId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rebalanceamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rebalanceamentos_cestas_top_five_CestaNovaId",
                        column: x => x.CestaNovaId,
                        principalTable: "cestas_top_five",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "contas_graficas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroConta = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contas_graficas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contas_graficas_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "eventos_ir_kafka",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    Cpf = table.Column<string>(type: "char(11)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenciaId = table.Column<long>(type: "bigint", nullable: true),
                    MesReferencia = table.Column<string>(type: "char(7)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValorOperacao = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    LucroLiquido = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    Aliquota = table.Column<decimal>(type: "decimal(8,6)", nullable: false),
                    ValorIr = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    PayloadJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Publicado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataPublicacao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_ir_kafka", x => x.Id);
                    table.ForeignKey(
                        name: "FK_eventos_ir_kafka_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "historico_valor_mensal",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    ValorAnterior = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    ValorNovo = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historico_valor_mensal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_historico_valor_mensal_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vendas_mes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    MesReferencia = table.Column<string>(type: "char(7)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoVenda = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    PrecoMedio = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    ValorTotalVenda = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Lucro = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Origem = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataVenda = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendas_mes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vendas_mes_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "distribuicoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrdemId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    ValorOperacao = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    ProporcaoCliente = table.Column<decimal>(type: "decimal(8,6)", nullable: false),
                    ValorIrDedoDuro = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    KafkaPublicado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataDistribuicao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_distribuicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_distribuicoes_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_distribuicoes_ordens_compra_OrdemId",
                        column: x => x.OrdemId,
                        principalTable: "ordens_compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "itens_ordem_compra",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrdemId = table.Column<long>(type: "bigint", nullable: false),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValorAlvo = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    CotacaoFechamento = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    QuantidadeCalculada = table.Column<int>(type: "int", nullable: false),
                    SaldoMasterDescontado = table.Column<int>(type: "int", nullable: false),
                    QuantidadeAComprar = table.Column<int>(type: "int", nullable: false),
                    QtdLotePadrao = table.Column<int>(type: "int", nullable: false),
                    QtdFracionario = table.Column<int>(type: "int", nullable: false),
                    TickerFracionario = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_ordem_compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_itens_ordem_compra_ordens_compra_OrdemId",
                        column: x => x.OrdemId,
                        principalTable: "ordens_compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rebalanceamento_clientes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RebalanceamentoId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalVendas = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    TotalCompras = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    IrDevido = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    KafkaIrPublicado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataExecucao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rebalanceamento_clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rebalanceamento_clientes_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rebalanceamento_clientes_rebalanceamentos_RebalanceamentoId",
                        column: x => x.RebalanceamentoId,
                        principalTable: "rebalanceamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "custodia_filhote",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    ContaGraficaId = table.Column<long>(type: "bigint", nullable: false),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoMedio = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custodia_filhote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_custodia_filhote_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_custodia_filhote_contas_graficas_ContaGraficaId",
                        column: x => x.ContaGraficaId,
                        principalTable: "contas_graficas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_Cpf",
                table: "clientes",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contas_graficas_ClienteId",
                table: "contas_graficas",
                column: "ClienteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contas_graficas_NumeroConta",
                table: "contas_graficas",
                column: "NumeroConta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cotacoes_historicas_DataPregao",
                table: "cotacoes_historicas",
                column: "DataPregao");

            migrationBuilder.CreateIndex(
                name: "IX_cotacoes_historicas_Ticker",
                table: "cotacoes_historicas",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_cotacoes_historicas_Ticker_DataPregao",
                table: "cotacoes_historicas",
                columns: new[] { "Ticker", "DataPregao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_custodia_filhote_ClienteId_Ticker",
                table: "custodia_filhote",
                columns: new[] { "ClienteId", "Ticker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_custodia_filhote_ContaGraficaId",
                table: "custodia_filhote",
                column: "ContaGraficaId");

            migrationBuilder.CreateIndex(
                name: "IX_custodia_master_Ticker",
                table: "custodia_master",
                column: "Ticker",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_distribuicoes_ClienteId",
                table: "distribuicoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_distribuicoes_OrdemId",
                table: "distribuicoes",
                column: "OrdemId");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_ir_kafka_ClienteId",
                table: "eventos_ir_kafka",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_historico_valor_mensal_ClienteId",
                table: "historico_valor_mensal",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_itens_cesta_CestaId_Ticker",
                table: "itens_cesta",
                columns: new[] { "CestaId", "Ticker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_itens_ordem_compra_OrdemId",
                table: "itens_ordem_compra",
                column: "OrdemId");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_compra_CestaId",
                table: "ordens_compra",
                column: "CestaId");

            migrationBuilder.CreateIndex(
                name: "IX_rebalanceamento_clientes_ClienteId",
                table: "rebalanceamento_clientes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_rebalanceamento_clientes_RebalanceamentoId",
                table: "rebalanceamento_clientes",
                column: "RebalanceamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_rebalanceamentos_CestaNovaId",
                table: "rebalanceamentos",
                column: "CestaNovaId");

            migrationBuilder.CreateIndex(
                name: "IX_vendas_mes_ClienteId_MesReferencia",
                table: "vendas_mes",
                columns: new[] { "ClienteId", "MesReferencia" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cotacoes_historicas");

            migrationBuilder.DropTable(
                name: "custodia_filhote");

            migrationBuilder.DropTable(
                name: "custodia_master");

            migrationBuilder.DropTable(
                name: "distribuicoes");

            migrationBuilder.DropTable(
                name: "eventos_ir_kafka");

            migrationBuilder.DropTable(
                name: "historico_valor_mensal");

            migrationBuilder.DropTable(
                name: "itens_cesta");

            migrationBuilder.DropTable(
                name: "itens_ordem_compra");

            migrationBuilder.DropTable(
                name: "rebalanceamento_clientes");

            migrationBuilder.DropTable(
                name: "vendas_mes");

            migrationBuilder.DropTable(
                name: "contas_graficas");

            migrationBuilder.DropTable(
                name: "ordens_compra");

            migrationBuilder.DropTable(
                name: "rebalanceamentos");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "cestas_top_five");
        }
    }
}
