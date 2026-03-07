import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getHistoricoCestas, type CestaResponse } from '../../api'

export default function HistoricoCestas() {
  const [cestas, setCestas] = useState<CestaResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [err, setErr] = useState<string | null>(null)
  const [expandido, setExpandido] = useState<number | null>(null)

  useEffect(() => {
    getHistoricoCestas()
      .then(setCestas)
      .catch(() => setErr('Erro ao carregar histórico de cestas.'))
      .finally(() => setLoading(false))
  }, [])

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Histórico de Cestas</h1>
        <Link to="/admin/cesta" className="btn-blue btn-sm">+ Nova Cesta</Link>
      </div>

      {loading && <div className="muted"><span className="spin" /> Carregando...</div>}
      {err && <div className="msg msg-err">{err}</div>}

      {!loading && cestas.length === 0 && (
        <div className="msg msg-info">Nenhuma cesta cadastrada ainda.</div>
      )}

      {cestas.map(c => (
        <div
          key={c.id}
          className="box"
          style={{ borderLeft: `3px solid ${c.ativa ? '#16a34a' : '#c4cdd8'}`, cursor: 'pointer' }}
          onClick={() => setExpandido(expandido === c.id ? null : c.id)}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <span className={`tag ${c.ativa ? 'tag-green' : 'tag-gray'}`}>
                {c.ativa ? 'Ativa' : 'Desativada'}
              </span>
              <strong>Cesta #{c.id}</strong>
              {c.criadoPor && <span className="muted">por {c.criadoPor}</span>}
            </div>
            <div className="muted" style={{ textAlign: 'right', fontSize: '.8rem' }}>
              <div>Ativada: {new Date(c.dataAtivacao).toLocaleString('pt-BR')}</div>
              {c.dataDesativacao && (
                <div>Desativada: {new Date(c.dataDesativacao).toLocaleString('pt-BR')}</div>
              )}
            </div>
          </div>

          <div style={{ marginTop: 10, display: 'flex', gap: 6, flexWrap: 'wrap' }}>
            {c.itens.map(it => (
              <span key={it.ticker} className="tag tag-blue">
                <span className="tk">{it.ticker}</span> {it.percentual}%
              </span>
            ))}
          </div>

          {expandido === c.id && (
            <div style={{ marginTop: 14 }}>
              <table>
                <thead>
                  <tr><th>Ticker</th><th>%</th><th>Distribuição</th></tr>
                </thead>
                <tbody>
                  {c.itens.map(it => (
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
            </div>
          )}

          <div className="muted" style={{ marginTop: 8, fontSize: '.78rem' }}>
            {expandido === c.id ? '▲ recolher' : '▼ ver detalhes'}
          </div>
        </div>
      ))}
    </div>
  )
}
