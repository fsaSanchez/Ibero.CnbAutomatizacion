import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'
import IberoWordmark from './IberoWordmark'

export default function Header() {
  const { isAdmin, isAuthenticated, user, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/personas')
  }

  return (
    <header style={{ backgroundColor: '#E00034' }} className="text-white shadow-md">
      <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between flex-wrap gap-2">
        <Link to="/personas" className="flex items-center gap-3 no-underline text-white">
          <IberoWordmark className="h-11 w-auto shrink-0" />
          <span className="w-px h-6 bg-white/30 hidden sm:block" />
          <span className="font-display font-bold text-lg leading-tight hidden sm:block">
            Sistema PUI
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

          {isAuthenticated ? (
            <>
              <span className="text-red-200 text-xs border border-red-300 rounded px-2 py-0.5">
                {user?.nombre ?? 'Administrador'}
              </span>
              <button
                onClick={handleLogout}
                className="bg-white/20 hover:bg-white/30 text-white text-sm px-3 py-1 rounded transition-colors cursor-pointer"
              >
                Cerrar sesión
              </button>
            </>
          ) : (
            <button
              onClick={() => navigate('/login')}
              className="bg-white/20 hover:bg-white/30 text-white text-sm px-3 py-1 rounded transition-colors cursor-pointer"
            >
              Iniciar sesión
            </button>
          )}
        </nav>
      </div>
    </header>
  )
}
