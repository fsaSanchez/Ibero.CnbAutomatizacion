import { useState, useCallback, useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { useAuth } from '../../hooks/useAuth'
import { API } from '../../constants/api'
import { authHeader } from '../../utilities/auth'
import { setResultado, setFiltrosAplicados, FILTROS_INICIALES } from '../../store/slices/personasSlice'
import TarjetaPersona from '../../components/PersonasDesaparecidas/TarjetaPersona'
import FiltrosPersonas from '../../components/PersonasDesaparecidas/FiltrosPersonas'
import Sidebar from '../../components/Common/Sidebar'

const TAMANO_PAGINA = 20

/** Ventana de números de página a mostrar alrededor de la página actual. */
function paginasVisibles(pagina, totalPaginas) {
  const rango = 2
  const inicio = Math.max(1, pagina - rango)
  const fin = Math.min(totalPaginas, pagina + rango)
  const paginas = []
  for (let p = inicio; p <= fin; p++) paginas.push(p)
  return paginas
}

export default function PersonasDesaparecidasPage() {
  const { isAdmin } = useAuth()
  const dispatch = useDispatch()
  const { datos, totalRegistros, totalPaginas, pagina, filtrosAplicados, cargado } = useSelector(s => s.personas)

  const [filtros, setFiltros] = useState(filtrosAplicados)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [mensaje, setMensaje] = useState(null)

  const cargar = useCallback(async (paginaActual, filtrosActuales) => {
    setLoading(true)
    setError(null)
    try {
      const params = new URLSearchParams({ pagina: paginaActual, tamanioPagina: TAMANO_PAGINA })
      if (filtrosActuales.folio) params.set('folio', filtrosActuales.folio)
      if (filtrosActuales.nombre) params.set('nombre', filtrosActuales.nombre)
      if (filtrosActuales.estado) params.set('estado', filtrosActuales.estado)
      if (filtrosActuales.fechaHechos) params.set('fechaHechos', filtrosActuales.fechaHechos)
      if (filtrosActuales.busqueda) params.set('busqueda', filtrosActuales.busqueda)

      const res = await fetch(`${API.personas}?${params}`)
      if (!res.ok) throw new Error(`Error ${res.status}`)
      const json = await res.json()
      const data = json.data ?? { datos: [], totalRegistros: 0, pagina: 1, tamanioPagina: TAMANO_PAGINA }
      dispatch(setResultado(data))
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }, [dispatch])

  // Solo se consulta la API si no hay datos ya cargados en memoria (p.ej. primera visita
  // de la sesión). Al volver desde el detalle de una persona, la lista sigue en Redux y
  // no se vuelve a pedir — los datos no cambiaron, así que no tiene sentido recargarlos.
  useEffect(() => {
    if (!cargado) cargar(pagina, filtrosAplicados)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  function handleCambioFiltro(campo, valor) {
    setFiltros(prev => ({ ...prev, [campo]: valor }))
  }

  function handleBuscar() {
    dispatch(setFiltrosAplicados(filtros))
    cargar(1, filtros)
  }

  function handleLimpiar() {
    setFiltros(FILTROS_INICIALES)
    dispatch(setFiltrosAplicados(FILTROS_INICIALES))
    cargar(1, FILTROS_INICIALES)
  }

  function handleCambiarPagina(nuevaPagina) {
    if (nuevaPagina < 1 || nuevaPagina > totalPaginas || nuevaPagina === pagina) return
    cargar(nuevaPagina, filtrosAplicados)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  async function handleEliminar(id) {
    try {
      const res = await fetch(API.eliminar(id), { method: 'DELETE', headers: authHeader() })
      const json = await res.json()
      setMensaje(json.message)
      cargar(pagina, filtrosAplicados)
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
      cargar(pagina, filtrosAplicados)
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
              {totalRegistros} {totalRegistros === 1 ? 'persona encontrada' : 'personas encontradas'}
            </p>

            {datos.length === 0 ? (
              <div className="text-center py-12 text-gray-400">
                No se encontraron personas con los filtros aplicados.
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 mb-6">
                {datos.map(p => (
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

            {totalPaginas > 1 && (
              <div className="flex items-center justify-center gap-2 py-4 flex-wrap">
                <button
                  onClick={() => handleCambiarPagina(pagina - 1)}
                  disabled={pagina === 1}
                  className="px-4 py-2 text-sm border rounded disabled:opacity-40 hover:bg-gray-50 cursor-pointer"
                >
                  ← Anterior
                </button>

                {paginasVisibles(pagina, totalPaginas)[0] > 1 && (
                  <span className="px-2 text-sm text-gray-400">…</span>
                )}

                {paginasVisibles(pagina, totalPaginas).map(p => (
                  <button
                    key={p}
                    onClick={() => handleCambiarPagina(p)}
                    className={`w-9 h-9 text-sm rounded border cursor-pointer ${
                      p === pagina
                        ? 'text-white border-transparent'
                        : 'hover:bg-gray-50 text-gray-700'
                    }`}
                    style={p === pagina ? { backgroundColor: '#E00034' } : {}}
                  >
                    {p}
                  </button>
                ))}

                {paginasVisibles(pagina, totalPaginas).at(-1) < totalPaginas && (
                  <span className="px-2 text-sm text-gray-400">…</span>
                )}

                <button
                  onClick={() => handleCambiarPagina(pagina + 1)}
                  disabled={pagina === totalPaginas}
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
