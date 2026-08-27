import { useState, useEffect, useCallback } from 'react'
import { useAuth } from '../../hooks/useAuth'
import { API } from '../../constants/api'
import { authHeader } from '../../utilities/auth'
import TarjetaPersona from '../../components/PersonasDesaparecidas/TarjetaPersona'
import FiltrosPersonas from '../../components/PersonasDesaparecidas/FiltrosPersonas'
import Sidebar from '../../components/Common/Sidebar'

const FILTROS_INICIALES = { folio: '', nombre: '', estado: '', fechaHechos: '', busqueda: '' }
const TAMANO_PAGINA = 20

export default function PersonasDesaparecidasPage() {
  const { isAdmin } = useAuth()
  const [filtros, setFiltros] = useState(FILTROS_INICIALES)
  const [filtrosAplicados, setFiltrosAplicados] = useState(FILTROS_INICIALES)
  const [pagina, setPagina] = useState(1)
  const [datos, setDatos] = useState({ datos: [], totalRegistros: 0, totalPaginas: 1 })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [mensaje, setMensaje] = useState(null)

  const cargar = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const params = new URLSearchParams({ pagina, tamanioPagina: TAMANO_PAGINA })
      if (filtrosAplicados.folio) params.set('folio', filtrosAplicados.folio)
      if (filtrosAplicados.nombre) params.set('nombre', filtrosAplicados.nombre)
      if (filtrosAplicados.estado) params.set('estado', filtrosAplicados.estado)
      if (filtrosAplicados.fechaHechos) params.set('fechaHechos', filtrosAplicados.fechaHechos)
      if (filtrosAplicados.busqueda) params.set('busqueda', filtrosAplicados.busqueda)

      const res = await fetch(`${API.personas}?${params}`)
      if (!res.ok) throw new Error(`Error ${res.status}`)
      const json = await res.json()
      setDatos(json.data ?? { datos: [], totalRegistros: 0, totalPaginas: 1 })
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }, [filtrosAplicados, pagina])

  useEffect(() => { cargar() }, [cargar])

  function handleCambioFiltro(campo, valor) {
    setFiltros(prev => ({ ...prev, [campo]: valor }))
  }

  function handleBuscar() {
    setFiltrosAplicados(filtros)
    setPagina(1)
  }

  function handleLimpiar() {
    setFiltros(FILTROS_INICIALES)
    setFiltrosAplicados(FILTROS_INICIALES)
    setPagina(1)
  }

  async function handleEliminar(id) {
    try {
      const res = await fetch(API.eliminar(id), { method: 'DELETE', headers: authHeader() })
      const json = await res.json()
      setMensaje(json.message)
      cargar()
    } catch {
      setMensaje('Error al eliminar')
    }
    setTimeout(() => setMensaje(null), 4000)
  }

  async function handlePublicar(id) {
    try {
      const res = await fetch(API.publicar(id), { method: 'POST', headers: authHeader() })
      const json = await res.json()
      setMensaje(json.message)
      cargar()
    } catch {
      setMensaje('Error al publicar')
    }
    setTimeout(() => setMensaje(null), 4000)
  }

  return (
    <div className="flex flex-1">
      <Sidebar />
      <div className="flex-1 max-w-7xl mx-auto px-4 py-6 w-full">
        <h2 className="text-xl font-bold font-display text-gray-800 mb-4">Personas Desaparecidas</h2>

        {mensaje && (
          <div className="mb-4 p-3 bg-green-50 border border-green-200 text-green-800 rounded text-sm">
            {mensaje}
          </div>
        )}

        <FiltrosPersonas
          filtros={filtros}
          onChange={handleCambioFiltro}
          onBuscar={handleBuscar}
          onLimpiar={handleLimpiar}
        />

        {loading && (
          <div className="text-center py-12 text-gray-500">Cargando...</div>
        )}

        {error && (
          <div className="text-center py-12 text-red-600">
            <p className="font-medium">No se pudo conectar con la API</p>
            <p className="text-sm text-gray-500 mt-1">{error}</p>
          </div>
        )}

        {!loading && !error && (
          <>
            <p className="text-sm text-gray-500 mb-4">
              {datos.totalRegistros} {datos.totalRegistros === 1 ? 'persona encontrada' : 'personas encontradas'}
            </p>

            {datos.datos.length === 0 ? (
              <div className="text-center py-12 text-gray-400">
                No se encontraron personas con los filtros aplicados.
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 mb-6">
                {datos.datos.map(p => (
                  <TarjetaPersona
                    key={p.idPersonaDesaparecida}
                    persona={p}
                    isAdmin={isAdmin}
                    onEliminar={handleEliminar}
                    onPublicar={handlePublicar}
                  />
                ))}
              </div>
            )}

            {datos.totalPaginas > 1 && (
              <div className="flex items-center justify-center gap-4 py-4">
                <button
                  onClick={() => setPagina(p => Math.max(1, p - 1))}
                  disabled={pagina === 1}
                  className="px-4 py-2 text-sm border rounded disabled:opacity-40 hover:bg-gray-50 cursor-pointer"
                >
                  ← Anterior
                </button>
                <span className="text-sm text-gray-600">
                  Página {pagina} de {datos.totalPaginas}
                </span>
                <button
                  onClick={() => setPagina(p => Math.min(datos.totalPaginas, p + 1))}
                  disabled={pagina === datos.totalPaginas}
                  className="px-4 py-2 text-sm border rounded disabled:opacity-40 hover:bg-gray-50 cursor-pointer"
                >
                  Siguiente →
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}
