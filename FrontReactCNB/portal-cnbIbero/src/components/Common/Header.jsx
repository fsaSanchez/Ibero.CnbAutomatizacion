import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'

export default function Header() {
  const { isAdmin, isAuthenticated, logout, toggleAdmin } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/')
  }

  return (
    <header style={{ backgroundColor: '#8B0000' }} className="text-white shadow-md">
      <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between flex-wrap gap-2">
        <Link to="/personas" className="flex items-center gap-3 no-underline text-white">
          <div className="w-8 h-8 bg-white rounded-full flex items-center justify-center">
            <span style={{ color: '#8B0000' }} className="font-bold text-sm">PUI</span>
          </div>
          <span className="font-semibold text-lg leading-tight hidden sm:block">
            Sistema PUI — Alertas IBERO
          </span>
        </Link>

        <nav className="flex items-center gap-4 flex-wrap text-sm">
          <Link to="/personas" className="text-white hover:text-red-200 transition-colors">
            Personas Desaparecidas
          </Link>

          {isAdmin && (
            <>
              <Link to="/admin/configuracion" className="text-white hover:text-red-200 transition-colors">
                Configuración
              </Link>
              <Link to="/admin/bitacora" className="text-white hover:text-red-200 transition-colors">
                Bitácora
              </Link>
            </>
          )}

          {isAuthenticated && (
            <span className="text-red-200 text-xs border border-red-300 rounded px-2 py-0.5">
              {isAdmin ? 'Admin' : 'Público'}
            </span>
          )}

          {import.meta.env.DEV && isAuthenticated && (
            <button
              onClick={toggleAdmin}
              className="text-xs border border-white/40 rounded px-2 py-0.5 hover:bg-white/10 transition-colors cursor-pointer"
              title="Toggle modo admin (solo dev)"
            >
              ⚙ {isAdmin ? 'Cambiar a Público' : 'Cambiar a Admin'}
            </button>
          )}

          {isAuthenticated && (
            <button
              onClick={handleLogout}
              className="bg-white/20 hover:bg-white/30 text-white text-sm px-3 py-1 rounded transition-colors cursor-pointer"
            >
              Cerrar sesión
            </button>
          )}
        </nav>
      </div>
    </header>
  )
}
