import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend,
} from 'recharts'
import { getCarteira, type CarteiraResponse } from '../api'

const CORES = ['#2563eb','#16a34a','#dc2626','#d97706','#7c3aed']

const brl = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })

const pct = (v: number) => `${v >= 0 ? '+' : ''}${v.toFixed(2)}%`

export default function Carteira() {
  const [params] = useSearchParams()
  const [id, setId] = useState(params.get('id') ?? '')
  const [dados, setDados] = useState<CarteiraResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [err, setErr] = useState<string | null>(null)

  async function buscar(e: React.FormEvent) {
    e.preventDefault()
    setErr(null); setDados(null); setLoading(true)
    try {
      const d = await getCarteira(Number(id))
      setDados(d)
    } catch (ex: unknown) {
      setErr((ex as {response?: {data?: {erro?: string}}})?.response?.data?.erro ?? 'Cliente não encontrado.')
    } finally { setLoading(false) }
  }

  const plPositivo = (dados?.plTotal ?? 0) >= 0

  return (
    <div>
      <div className="page-header">
        <h1>Carteira do Cliente</h1>
      </div>

      <div className="box">
        <form onSubmit={buscar} style={{ display: 'flex', gap: 12, alignItems: 'flex-end' }}>
          <div className="field" style={{ margin: 0, width: 200 }}>
            <label>ID do cliente</label>
            <input required type="number" min={1} value={id} onChange={e => setId(e.target.value)} placeholder="Ex: 1" />
          </div>
          <button className="btn-blue" type="submit" disabled={loading}>
            {loading ? <><span className="spin" /> Buscando...</> : 'Consultar'}
          </button>
        </form>
      </div>

      {err && <div className="msg msg-err">{err}</div>}

      {dados && (
        <>
          <div className="box" style={{ borderLeft: '3px solid #2563eb' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8 }}>
              <div>
                <div style={{ fontSize: '1.2rem', fontWeight: 700 }}>{dados.nome}</div>
                <div className="muted" style={{ marginTop: 2 }}>Cliente #{dados.clienteId}</div>
              </div>
              <span
                className={`tag ${plPositivo ? 'tag-green' : ''}`}
                style={!plPositivo ? { background: '#fee2e2', color: '#991b1b' } : {}}
              >
                P/L: {brl(dados.plTotal)}
              </span>
            </div>
          </div>

          <div className="kpi-grid">
            <div className="kpi">
              <div className="kpi-label">Total Investido</div>
              <div className="kpi-value blue">{brl(dados.valorInvestidoTotal)}</div>
            </div>
            <div className="kpi">
              <div className="kpi-label">Valor Atual</div>
              <div className="kpi-value blue">{brl(dados.valorAtualTotal)}</div>
            </div>
            <div className="kpi">
              <div className="kpi-label">P/L Total</div>
              <div className={`kpi-value ${plPositivo ? 'green' : 'red'}`}>{brl(dados.plTotal)}</div>
            </div>
            <div className="kpi">
              <div className="kpi-label">Rentabilidade</div>
              <div className={`kpi-value ${plPositivo ? 'green' : 'red'}`}>{pct(dados.rentabilidadePercent)}</div>
            </div>
          </div>

          {dados.ativos.length === 0 ? (
            <div className="msg msg-info">Este cliente ainda não possui ativos na carteira.</div>
          ) : (
            <>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 18 }}>
                <div className="box">
                  <h2>P/L por Ativo (R$)</h2>
                  <ResponsiveContainer width="100%" height={210}>
                    <BarChart data={dados.ativos} margin={{ top: 4, right: 8, bottom: 4, left: 0 }}>
                      <XAxis dataKey="ticker" tick={{ fontSize: 11 }} />
                      <YAxis tick={{ fontSize: 11 }} tickFormatter={v => brl(v).replace('R$\u00a0', '')} width={70} />
                      <Tooltip formatter={(v: number) => brl(v)} />
                      <Bar dataKey="pl" name="P/L">
                        {dados.ativos.map((a, i) => (
                          <Cell key={a.ticker} fill={a.pl >= 0 ? CORES[i % CORES.length] : '#dc2626'} />
                        ))}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                </div>

                <div className="box">
                  <h2>Composição (%)</h2>
                  <ResponsiveContainer width="100%" height={210}>
                    <PieChart>
                      <Pie
                        data={dados.ativos}
                        dataKey="composicaoPercent"
                        nameKey="ticker"
                        cx="50%" cy="50%"
                        outerRadius={75}
                        label={({ name, value }: { name: string; value: number }) => `${name} ${value.toFixed(1)}%`}
                        labelLine={false}
                      >
                        {dados.ativos.map((_, i) => (
                          <Cell key={i} fill={CORES[i % CORES.length]} />
                        ))}
                      </Pie>
                      <Legend iconSize={11} />
                      <Tooltip formatter={(v: number) => `${v.toFixed(2)}%`} />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              </div>

              <div className="box">
                <h2>Posição Detalhada</h2>
                <div style={{ overflowX: 'auto' }}>
                  <table>
                    <thead>
                      <tr>
                        <th>Ticker</th>
                        <th>Qtd</th>
                        <th>Preço Médio</th>
                        <th>Cotação Atual</th>
                        <th>Valor Atual</th>
                        <th>P/L</th>
                        <th>Rentab.</th>
                        <th>Composição</th>
                      </tr>
                    </thead>
                    <tbody>
                      {dados.ativos.map(a => {
                        const rentab = a.precoMedio > 0 ? ((a.cotacaoAtual / a.precoMedio) - 1) * 100 : 0
                        return (
                          <tr key={a.ticker}>
                            <td><span className="tk">{a.ticker}</span></td>
                            <td>{a.quantidade.toLocaleString('pt-BR')}</td>
                            <td>{brl(a.precoMedio)}</td>
                            <td>{brl(a.cotacaoAtual)}</td>
                            <td>{brl(a.valorAtual)}</td>
                            <td style={{ color: a.pl >= 0 ? '#15803d' : '#dc2626', fontWeight: 600 }}>
                              {brl(a.pl)}
                            </td>
                            <td style={{ color: rentab >= 0 ? '#15803d' : '#dc2626' }}>
                              {pct(rentab)}
                            </td>
                            <td>
                              <div style={{ display: 'flex', alignItems: 'center', gap: 7 }}>
                                <div className="pbar" style={{ width: 70 }}>
                                  <div className="pbar-fill" style={{ width: `${a.composicaoPercent}%` }} />
                                </div>
                                {a.composicaoPercent.toFixed(1)}%
                              </div>
                            </td>
                          </tr>
                        )
                      })}
                    </tbody>
                  </table>
                </div>
              </div>
            </>
          )}
        </>
      )}
    </div>
  )
}
