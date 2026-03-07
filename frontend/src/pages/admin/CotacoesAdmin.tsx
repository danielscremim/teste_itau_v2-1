import { useState } from 'react'
import { importarCotacoes } from '../../api'

export default function CotacoesAdmin() {
  const [arquivo, setArquivo] = useState('')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<Record<string, unknown> | null>(null)
  const [err, setErr] = useState<string | null>(null)

  async function handleImportar(e: React.FormEvent) {
    e.preventDefault()
    setErr(null); setResult(null); setLoading(true)
    try {
      const r = await importarCotacoes(arquivo)
      setResult(r)
    } catch (ex: unknown) {
      setErr((ex as {response?: {data?: {erro?: string}}})?.response?.data?.erro ?? 'Erro ao importar cotações.')
    } finally { setLoading(false) }
  }

  const hoje = new Date()
  const ymd = `${hoje.getFullYear()}${String(hoje.getMonth()+1).padStart(2,'0')}${String(hoje.getDate()).padStart(2,'0')}`
  const sugestao = `COTAHIST_D${ymd}.TXT`

  return (
    <div>
      <div className="page-header">
        <h1>Importar Cotações COTAHIST</h1>
      </div>

      <div className="box">
        <h2>Como funciona</h2>
        <div className="msg msg-info" style={{ marginBottom: 0 }}>
          <ol style={{ paddingLeft: 18, lineHeight: 2, fontSize: '.855rem' }}>
            <li>Baixe o arquivo COTAHIST do site da B3 (formato TXT, diário)</li>
            <li>Coloque-o na pasta <code>cotacoes/</code> na raiz do projeto</li>
            <li>Informe o nome do arquivo abaixo e clique em Importar</li>
          </ol>
        </div>
      </div>

      <div className="box">
        <h2>Importar arquivo</h2>
        {err && <div className="msg msg-err">{err}</div>}
        {result && (
          <div className="msg msg-ok">
            Importado com sucesso — {String(result.totalRegistros)} registros.<br />
            Arquivo: <code>{String(result.nomeArquivo)}</code>
          </div>
        )}
        <form onSubmit={handleImportar}>
          <div className="field" style={{ maxWidth: 380 }}>
            <label>Nome do arquivo</label>
            <input
              required
              value={arquivo}
              onChange={e => setArquivo(e.target.value)}
              placeholder={sugestao}
            />
          </div>
          <div style={{ marginBottom: 14 }}>
            <button type="button" className="btn-ghost btn-sm" onClick={() => setArquivo(sugestao)}>
              Usar nome de hoje ({sugestao})
            </button>
          </div>
          <button className="btn-blue" type="submit" disabled={loading}>
            {loading ? <><span className="spin" /> Importando...</> : 'Importar'}
          </button>
        </form>
      </div>

      <div className="box">
        <h2>Onde baixar o COTAHIST</h2>
        <p className="muted">
          Acesse B3 → Market Data → Cotações Históricas → selecione o dia desejado e baixe o arquivo TXT.
        </p>
      </div>
    </div>
  )
}
