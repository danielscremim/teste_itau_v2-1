import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom'
import Home            from './pages/Home'
import Adesao          from './pages/Adesao'
import Carteira        from './pages/Carteira'
import CestaAdmin      from './pages/admin/CestaAdmin'
import CotacoesAdmin   from './pages/admin/CotacoesAdmin'
import MotorAdmin      from './pages/admin/MotorAdmin'
import HistoricoCestas from './pages/admin/HistoricoCestas'

export default function App() {
  return (
    <BrowserRouter>
      <div className="layout">
        <aside className="sidebar">
          <div className="sidebar-logo">
            Compra Programada
            <small>Itaú Corretora</small>
          </div>
          <nav>
            <div className="nav-section-label">Geral</div>
            <NavLink to="/" end>Dashboard</NavLink>

            <div className="nav-section-label">Cliente</div>
            <NavLink to="/adesao">Gestão de Clientes</NavLink>
            <NavLink to="/carteira">Carteira</NavLink>

            <div className="nav-section-label">Operações</div>
            <NavLink to="/admin/cesta">Cesta Top Five</NavLink>
            <NavLink to="/admin/cotacoes">Cotações COTAHIST</NavLink>
            <NavLink to="/admin/motor">Motor de Compras</NavLink>
            <NavLink to="/admin/historico">Histórico de Cestas</NavLink>
          </nav>
        </aside>

        <main className="content">
          <Routes>
            <Route path="/"                element={<Home />} />
            <Route path="/adesao"          element={<Adesao />} />
            <Route path="/carteira"        element={<Carteira />} />
            <Route path="/admin/cesta"     element={<CestaAdmin />} />
            <Route path="/admin/cotacoes"  element={<CotacoesAdmin />} />
            <Route path="/admin/motor"     element={<MotorAdmin />} />
            <Route path="/admin/historico" element={<HistoricoCestas />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}
