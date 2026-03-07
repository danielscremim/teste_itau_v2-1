import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { getCestaAtiva, cadastrarCesta, type CestaResponse } from '../../api'

interface ItemForm { ticker: string; percentual: number }

const defaultItens = (): ItemForm[] => [
  { ticker: 'PETR4', percentual: 30 },
  { ticker: 'VALE3', percentual: 25 },
  { ticker: 'ITUB4', percentual: 20 },
  { ticker: 'B3SA3', percentual: 15 },
  { ticker: 'ABEV3', percentual: 10 },
]

export default function CestaAdmin() {
  const [cestaAtiva, setCestaAtiva] = useState<CestaResponse | null>(null)
  const [loadingCesta, setLoadingCesta] = useState(true)
  const [errCesta, setErrCesta] = useState<string | null>(null)

  const [itens, setItens] = useState<ItemForm[]>(defaultItens())
  const [criadoPor, setCriadoPor] = useState('')
  const [loadingSalvar, setLoadingSalvar] = useState(false)
  const [result, setResult] = useState<CestaResponse | null>(null)
  const [errSalvar, setErrSalvar] = useState<string | null>(null)

  const soma = itens.reduce((s, i) => s + (i.percentual || 0), 0)
  const somaOk = Math.abs(soma - 100) < 0.001

  useEffect(() => {
    getCestaAtiva()
      .then(setCestaAtiva)
      .catch(() => setErrCesta('Nenhuma cesta ativa no momento.'))
      .finally(() => setLoadingCesta(false))
  }, [result])

  function setItem(idx: number, field: keyof ItemForm, value: string | number) {
    setItens(prev => prev.map((it, i) => i === idx ? { ...it, [field]: value } : it))
  }

  async function handleSalvar(e: React.FormEvent) {
    e.preventDefault()
    if (!somaOk) return
    setErrSalvar(null); setResult(null); setLoadingSalvar(true)
    try {
      const r = await cadastrarCesta({
        itens: itens.map(i => ({ ticker: i.ticker.toUpperCase(), percentual: i.percentual })),
        criadoPor: criadoPor || undefined,
      })
      setResult(r)
    } catch (ex: unknown) {
      setErrSalvar((ex as {response?: {data?: {erro?: string}}})?.response?.data?.erro ?? 'Erro ao salvar cesta.')
    } finally { setLoadingSalvar(false) }
  }

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Cesta Top Five</h1>
        <Link to="/admin/historico" className="btn-ghost btn-sm">Ver histórico</Link>
      </div>

      <div className="box">
        <h2>Cesta Ativa</h2>
        {loadingCesta ? (
          <div className="muted"><span className="spin" /> Carregando...</div>
        ) : errCesta ? (
          <div className="msg msg-info">{errCesta}</div>
        ) : cestaAtiva && (
          <>
            <p className="muted" style={{ marginBottom: 12 }}>
              Cadastrada em {new Date(cestaAtiva.dataAtivacao).toLocaleString('pt-BR')}
              {cestaAtiva.criadoPor ? ` por ${cestaAtiva.criadoPor}` : ''}
            </p>
            <table>
              <thead>
                <tr><th>Ticker</th><th>%</th><th>Distribuição</th></tr>
              </thead>
              <tbody>
                {cestaAtiva.itens.map(it => (
                  <tr key={it.ticker}>
                    <td><span className="tk">{it.ticker}</span></td>
                    <td>{it.percentual.toFixed(1)}%</td>
                    <td>
                      <div className="pbar" style={{ width: 180 }}>
                        <div className="pbar-fill" style={{ width: `${it.percentual}%` }} />
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}
      </div>

      <div className="box">
        <h2>Cadastrar Nova Cesta</h2>
        <p className="muted" style={{ marginBottom: 14 }}>
          Ao salvar, a cesta anterior será desativada automaticamente.
        </p>
        {errSalvar && <div className="msg msg-err">{errSalvar}</div>}
        {result && <div className="msg msg-ok">Cesta cadastrada com sucesso. ID: {result.id}</div>}

        <form onSubmit={handleSalvar}>
          {itens.map((it, i) => (
            <div key={i} className="cesta-row">
              <span className="muted" style={{ width: 18 }}>{i + 1}.</span>
              <input
                type="text"
                placeholder="TICKER"
                maxLength={8}
                value={it.ticker}
                onChange={e => setItem(i, 'ticker', e.target.value.toUpperCase())}
              />
              <input
                type="number"
                min={0.01}
                max={100}
                step={0.01}
                value={it.percentual}
                onChange={e => setItem(i, 'percentual', Number(e.target.value))}
              />
              <span className="muted">%</span>
              <div className="pbar" style={{ width: 120 }}>
                <div className="pbar-fill" style={{ width: `${Math.min(it.percentual, 100)}%` }} />
              </div>
            </div>
          ))}

          <div style={{
            margin: '10px 0 18px',
            fontSize: '.855rem',
            fontWeight: 600,
            color: somaOk ? '#15803d' : '#dc2626',
          }}>
            Soma: {soma.toFixed(2)}% {!somaOk && `(faltam ${(100 - soma).toFixed(2)}%)`}
          </div>

          <div className="field" style={{ maxWidth: 280 }}>
            <label>Criado por (opcional)</label>
            <input value={criadoPor} onChange={e => setCriadoPor(e.target.value)} placeholder="Ex: Gestor Research" />
          </div>

          <button className="btn-green" type="submit" disabled={!somaOk || loadingSalvar}>
            {loadingSalvar ? <><span className="spin" /> Salvando...</> : 'Salvar Nova Cesta'}
          </button>
        </form>
      </div>
    </div>
  )
}
