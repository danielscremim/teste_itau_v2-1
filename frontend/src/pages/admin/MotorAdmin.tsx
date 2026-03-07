import { useState } from 'react'
import { executarMotor } from '../../api'

const brl = (v: number) => v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })

export default function MotorAdmin() {
  const hoje = new Date().toISOString().split('T')[0]
  const [data, setData] = useState(hoje)
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<Record<string, unknown> | null>(null)
  const [err, setErr] = useState<string | null>(null)

  async function handleExecutar(e: React.FormEvent) {
    e.preventDefault()
    setErr(null); setResult(null); setLoading(true)
    try {
      const r = await executarMotor(data)
      setResult(r)
    } catch (ex: unknown) {
      setErr((ex as {response?: {data?: {erro?: string}}})?.response?.data?.erro ?? 'Erro ao executar motor.')
    } finally { setLoading(false) }
  }

  type ItemMotor = {
    ticker: string
    valorAlvo: number
    cotacaoFechamento: number
    quantidadeCalculada: number
    saldoMasterDescontado: number
    quantidadeComprada: number
    qtdLotePadrao: number
    qtdFracionario: number
  }

  const itens = result?.itens as ItemMotor[] | undefined

  return (
    <div>
      <div className="page-header">
        <h1>Motor de Compras</h1>
      </div>

      <div className="box">
        <h2>Como funciona</h2>
        <div className="msg msg-info" style={{ marginBottom: 0 }}>
          O motor executa no <strong>5º, 15º e 25º dia útil</strong> de cada mês:
          <ol style={{ paddingLeft: 18, marginTop: 8, lineHeight: 1.9, fontSize: '.855rem' }}>
            <li>Coleta todos os clientes ativos e calcula <strong>1/3 do valor mensal</strong> de cada um</li>
            <li>Consolida e calcula quantas ações comprar (cotação de fechamento do COTAHIST)</li>
            <li>Desconta saldo remanescente da Custódia Master</li>
            <li>Distribui os ativos proporcionalmente para cada Custódia Filhote</li>
            <li>Publica o IR dedo-duro (0,005%) no Kafka</li>
          </ol>
        </div>
      </div>

      <div className="box">
        <h2>Executar ciclo</h2>
        {err && <div className="msg msg-err">{err}</div>}
        <form onSubmit={handleExecutar}>
          <div className="field" style={{ maxWidth: 220 }}>
            <label>Data de Referência</label>
            <input
              type="date"
              value={data}
              onChange={e => setData(e.target.value)}
            />
          </div>
          <button className="btn-green" type="submit" disabled={loading}>
            {loading ? <><span className="spin" /> Executando...</> : 'Executar Motor'}
          </button>
        </form>
      </div>

      {result && (
        <div className="box">
          <h2>Resultado</h2>
          <div className="kpi-grid" style={{ marginBottom: 18 }}>
            <div className="kpi">
              <div className="kpi-label">Status</div>
              <div className="kpi-value" style={{ fontSize: '1rem', marginTop: 6 }}>
                <span className={`tag ${String(result.status) === 'Executado' ? 'tag-green' : 'tag-gray'}`}>
                  {String(result.status)}
                </span>
              </div>
            </div>
            <div className="kpi">
              <div className="kpi-label">Data Referência</div>
              <div className="kpi-value" style={{ fontSize: '1rem' }}>{String(result.dataReferencia)}</div>
            </div>
            <div className="kpi">
              <div className="kpi-label">Total Consolidado</div>
              <div className="kpi-value blue">{brl(Number(result.totalConsolidado))}</div>
            </div>
            <div className="kpi">
              <div className="kpi-label">Clientes Ativos</div>
              <div className="kpi-value">{String(result.totalClientesAtivos)}</div>
            </div>
          </div>

          {itens && itens.length > 0 && (
            <>
              <h3 style={{ fontSize: '.9rem', fontWeight: 600, marginBottom: 12, color: '#1c2d3e' }}>
                Compras Realizadas
              </h3>
              <div style={{ overflowX: 'auto' }}>
                <table>
                  <thead>
                    <tr>
                      <th>Ticker</th>
                      <th>Valor Alvo</th>
                      <th>Cotação</th>
                      <th>Qtd Calculada</th>
                      <th>Saldo Master</th>
                      <th>Qtd Comprada</th>
                      <th>Lote Padrão</th>
                      <th>Fracionário</th>
                    </tr>
                  </thead>
                  <tbody>
                    {itens.map(it => (
                      <tr key={it.ticker}>
                        <td><span className="tk">{it.ticker}</span></td>
                        <td>{brl(it.valorAlvo)}</td>
                        <td>{brl(it.cotacaoFechamento)}</td>
                        <td>{it.quantidadeCalculada}</td>
                        <td style={{ color: it.saldoMasterDescontado > 0 ? '#15803d' : '#5e7182' }}>
                          {it.saldoMasterDescontado}
                        </td>
                        <td><strong>{it.quantidadeComprada}</strong></td>
                        <td>{it.qtdLotePadrao > 0 ? <span className="tag tag-blue">{it.qtdLotePadrao}</span> : '–'}</td>
                        <td>{it.qtdFracionario > 0 ? <span className="tag tag-green">{it.qtdFracionario}F</span> : '–'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  )
}
