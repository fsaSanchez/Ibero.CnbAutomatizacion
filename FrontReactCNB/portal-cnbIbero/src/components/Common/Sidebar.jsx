import { NavLink } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'

export default function Sidebar() {
  const { isAdmin } = useAuth()
  if (!isAdmin) return null

  const linkClass = ({ isActive }) =>
    `block px-4 py-2 rounded text-sm transition-colors ${
      isActive
        ? 'bg-white/20 text-white font-semibold'
        : 'text-red-100 hover:bg-white/10'
    }`

  return (
    <aside style={{ backgroundColor: '#E00034' }} className="w-48 shrink-0 min-h-full p-4 space-y-1">
      <p className="text-red-200 text-xs uppercase tracking-wider mb-3">Administración</p>
      <NavLink to="/personas" className={linkClass}>Personas</NavLink>
      <NavLink to="/admin/cargar-pdf" className={linkClass}>Cargar PDF</NavLink>
      <NavLink to="/admin/configuracion" className={linkClass}>Configuración</NavLink>
      <NavLink to="/admin/bitacora" className={linkClass}>Bitácora</NavLink>
      <NavLink to="/admin/cese-difusion" className={linkClass}>Cese de difusión</NavLink>
    </aside>
  )
}
