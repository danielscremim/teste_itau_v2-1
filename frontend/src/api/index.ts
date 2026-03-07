import axios from 'axios'

const api = axios.create({ baseURL: '/api' })

// ── CLIENTES ─────────────────────────────────────────────────────────────────

export interface AdesaoRequest {
  nome: string
  cpf: string
  email: string
  valorMensal: number
}

export interface AlterarValorRequest {
  novoValorMensal: number
}

export interface AtivoCarteira {
  ticker: string
  quantidade: number
  precoMedio: number
  cotacaoAtual: number
  valorAtual: number
  pl: number
  composicaoPercent: number
}

export interface CarteiraResponse {
  clienteId: number
  nome: string
  valorInvestidoTotal: number
  valorAtualTotal: number
  plTotal: number
  rentabilidadePercent: number
  ativos: AtivoCarteira[]
}

export const aderir = (body: AdesaoRequest) =>
  api.post('/clientes/adesao', body).then(r => r.data)

export const sair = (id: number) =>
  api.delete(`/clientes/${id}/saida`).then(r => r.data)

export const alterarValor = (id: number, body: AlterarValorRequest) =>
  api.patch(`/clientes/${id}/valor-mensal`, body).then(r => r.data)

export const getCarteira = (id: number): Promise<CarteiraResponse> =>
  api.get(`/clientes/${id}/carteira`).then(r => r.data)

// ── ADMIN ─────────────────────────────────────────────────────────────────────

export interface ItemCestaReq { ticker: string; percentual: number }
export interface CadastrarCestaRequest { itens: ItemCestaReq[]; criadoPor?: string }

export interface ItemCestaResp { ticker: string; percentual: number }
export interface CestaResponse {
  id: number
  ativa: boolean
  dataAtivacao: string
  dataDesativacao: string | null
  criadoPor: string | null
  itens: ItemCestaResp[]
}

export const getCestaAtiva = (): Promise<CestaResponse> =>
  api.get('/admin/cesta').then(r => r.data)

export const getHistoricoCestas = (): Promise<CestaResponse[]> =>
  api.get('/admin/cesta/historico').then(r => r.data)

export const cadastrarCesta = (body: CadastrarCestaRequest) =>
  api.post('/admin/cesta', body).then(r => r.data)

export const importarCotacoes = (nomeArquivo: string) =>
  api.post('/admin/cotacoes/importar', { nomeArquivo }).then(r => r.data)

export const executarMotor = (dataReferencia?: string) =>
  api.post('/admin/motor-compra', { dataReferencia }).then(r => r.data)
