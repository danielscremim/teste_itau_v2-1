import { Link } from 'react-router-dom'

export default function Home() {
  return (
    <div>
      <div className="page-header">
        <h1>Dashboard</h1>
        <p>Sistema de Compra Programada de Ações — Itaú Corretora</p>
      </div>

      <div className="home-tiles">
        <div className="box">
          <h2>Clientes</h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <Link to="/adesao" style={{ color: '#2563eb', fontSize: '.875rem' }}>Gestão de clientes (adesão / saída / alteração)</Link>
            <Link to="/carteira" style={{ color: '#2563eb', fontSize: '.875rem' }}>Consultar carteira e rentabilidade</Link>
          </div>
        </div>

        <div className="box">
          <h2>Operações</h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <Link to="/admin/motor" style={{ color: '#2563eb', fontSize: '.875rem' }}>Executar motor de compras</Link>
            <Link to="/admin/cesta" style={{ color: '#2563eb', fontSize: '.875rem' }}>Cesta Top Five</Link>
            <Link to="/admin/cotacoes" style={{ color: '#2563eb', fontSize: '.875rem' }}>Importar cotações B3</Link>
            <Link to="/admin/historico" style={{ color: '#2563eb', fontSize: '.875rem' }}>Histórico de cestas</Link>
          </div>
        </div>
      </div>

      <div className="box" style={{ maxWidth: 780 }}>
        <h2>Fluxo de uso</h2>
        <ol style={{ paddingLeft: 20, lineHeight: 2, fontSize: '.855rem', color: '#4a5e6e' }}>
          <li>Importe o arquivo COTAHIST da B3 com as cotações do dia</li>
          <li>Confirme que há uma Cesta Top Five ativa cadastrada</li>
          <li>Cadastre os clientes e defina o valor mensal de aporte</li>
          <li>Execute o motor de compras na data de referência</li>
          <li>Consulte a carteira de cada cliente para ver os ativos distribuídos</li>
        </ol>
      </div>
    </div>
  )
}
