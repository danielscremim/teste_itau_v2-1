import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { aderir, sair, alterarValor, type AdesaoRequest } from '../api'

export default function Adesao() {
  const nav = useNavigate()
  const [tab, setTab] = useState<'aderir' | 'sair' | 'alterar'>('aderir')

  const [form, setForm] = useState<AdesaoRequest>({ nome: '', cpf: '', email: '', valorMensal: 500 })
  const [resultAdesao, setResultAdesao] = useState<Record<string, unknown> | null>(null)

  const [idSair, setIdSair] = useState('')
  const [resultSair, setResultSair] = useState<Record<string, unknown> | null>(null)

  const [idAlt, setIdAlt] = useState('')
  const [novoValor, setNovoValor] = useState(500)
  const [resultAlt, setResultAlt] = useState<Record<string, unknown> | null>(null)

  const [loading, setLoading] = useState(false)
  const [err, setErr] = useState<string | null>(null)

  const reset = () => { setErr(null); setResultAdesao(null); setResultSair(null); setResultAlt(null) }

  async function handleAderir(e: React.FormEvent) {
    e.preventDefault(); reset(); setLoading(true)
    try {
      const r = await aderir(form)
      setResultAdesao(r)
    } catch (ex: unknown) {
      setErr((ex as {response?: {data?: {erro?: string}}})?.response?.data?.erro ?? 'Erro ao processar adesão.')
    } finally { setLoading(false) }
  }

  async function handleSair(e: React.FormEvent) {
    e.preventDefault(); reset(); setLoading(true)
    try {
      const r = await sair(Number(idSair))
      setResultSair(r)
    } catch (ex: unknown) {
      setErr((ex as {response?: {data?: {erro?: string}}})?.response?.data?.erro ?? 'Erro ao processar saída.')
    } finally { setLoading(false) }
  }

  async function handleAlterar(e: React.FormEvent) {
    e.preventDefault(); reset(); setLoading(true)
    try {
      const r = await alterarValor(Number(idAlt), { novoValorMensal: novoValor })
      setResultAlt(r)
    } catch (ex: unknown) {
      setErr((ex as {response?: {data?: {erro?: string}}})?.response?.data?.erro ?? 'Erro ao alterar valor.')
    } finally { setLoading(false) }
  }

  return (
    <div>
      <div className="page-header">
        <h1>Gestão de Clientes</h1>
      </div>

      <div className="tabs">
        {(['aderir', 'sair', 'alterar'] as const).map(t => (
          <button
            key={t}
            className={`tab-btn${tab === t ? ' active' : ''}`}
            onClick={() => { setTab(t); reset() }}
          >
            {t === 'aderir' ? 'Nova Adesão' : t === 'sair' ? 'Saída do Produto' : 'Alterar Valor'}
          </button>
        ))}
      </div>

      {err && <div className="msg msg-err">{err}</div>}

      {tab === 'aderir' && (
        <div className="box">
          <h2>Cadastrar novo cliente</h2>
          <form onSubmit={handleAderir}>
            <div className="row">
              <div className="field">
                <label>Nome completo</label>
                <input required value={form.nome} onChange={e => setForm({...form, nome: e.target.value})} placeholder="Ex: João Silva" />
              </div>
              <div className="field">
                <label>CPF (só dígitos)</label>
                <input required maxLength={11} value={form.cpf} onChange={e => setForm({...form, cpf: e.target.value})} placeholder="12345678901" />
              </div>
            </div>
            <div className="row">
              <div className="field">
                <label>E-mail</label>
                <input required type="email" value={form.email} onChange={e => setForm({...form, email: e.target.value})} placeholder="joao@email.com" />
              </div>
              <div className="field">
                <label>Valor mensal (R$)</label>
                <input required type="number" min={100} step={50} value={form.valorMensal} onChange={e => setForm({...form, valorMensal: Number(e.target.value)})} />
              </div>
            </div>
            <button className="btn-blue" type="submit" disabled={loading}>
              {loading ? <><span className="spin" /> Processando...</> : 'Confirmar Adesão'}
            </button>
          </form>

          {resultAdesao && (
            <div className="msg msg-ok" style={{ marginTop: 14 }}>
              Adesão realizada. ID do cliente: <strong>{String(resultAdesao.clienteId)}</strong>{' '}|{' '}
              Conta gráfica: <strong>{String(resultAdesao.numeroConta)}</strong>
              <div style={{ marginTop: 8 }}>
                <button className="btn-ghost btn-sm" onClick={() => nav(`/carteira?id=${String(resultAdesao.clienteId)}`)}>
                  Ver carteira →
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {tab === 'sair' && (
        <div className="box">
          <h2>Cancelar adesão de um cliente</h2>
          <p className="muted" style={{ marginBottom: 14 }}>
            O cliente será desativado e não receberá novas compras. A posição existente é mantida.
          </p>
          <form onSubmit={handleSair}>
            <div className="field" style={{ maxWidth: 220 }}>
              <label>ID do cliente</label>
              <input required type="number" min={1} value={idSair} onChange={e => setIdSair(e.target.value)} placeholder="Ex: 1" />
            </div>
            <button className="btn-red" type="submit" disabled={loading}>
              {loading ? <><span className="spin" /> Processando...</> : 'Confirmar Saída'}
            </button>
          </form>
          {resultSair && (
            <div className="msg msg-ok" style={{ marginTop: 14 }}>
              Cliente desativado com sucesso.
            </div>
          )}
        </div>
      )}

      {tab === 'alterar' && (
        <div className="box">
          <h2>Alterar valor mensal de aporte</h2>
          <form onSubmit={handleAlterar}>
            <div className="row">
              <div className="field" style={{ maxWidth: 200 }}>
                <label>ID do cliente</label>
                <input required type="number" min={1} value={idAlt} onChange={e => setIdAlt(e.target.value)} placeholder="Ex: 1" />
              </div>
              <div className="field" style={{ maxWidth: 200 }}>
                <label>Novo valor mensal (R$)</label>
                <input required type="number" min={100} step={50} value={novoValor} onChange={e => setNovoValor(Number(e.target.value))} />
              </div>
            </div>
            <button className="btn-blue" type="submit" disabled={loading}>
              {loading ? <><span className="spin" /> Salvando...</> : 'Salvar Alteração'}
            </button>
          </form>
          {resultAlt && (
            <div className="msg msg-ok" style={{ marginTop: 14 }}>
              Valor alterado. Anterior: <strong>R$ {Number(resultAlt.valorAnterior).toFixed(2)}</strong> →{' '}
              Novo: <strong>R$ {Number(resultAlt.valorAtual).toFixed(2)}</strong>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

