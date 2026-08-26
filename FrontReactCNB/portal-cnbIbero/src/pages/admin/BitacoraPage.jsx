import { useState, useEffect } from 'react'
import { API } from '../../constants/api'
import { authHeader } from '../../utilities/auth'
import Sidebar from '../../components/Common/Sidebar'

const TABS = [
  { id: 'publicaciones', label: 'Publicaciones Facebook' },
  { id: 'general', label: 'Bitácora general' },
]

const PAGE_SIZE = 15

function TablaPublicaciones({ registros }) {
  if (registros.length === 0) return <p className="text-center py-8 text-gray-400">Sin registros.</p>
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b text-left text-xs text-gray-500 uppercase tracking-wide">
            <th className="pb-2 pr-4">Fecha</th>
            <th className="pb-2 pr-4">Persona</th>
            <th className="pb-2 pr-4">Tipo</th>
            <th className="pb-2 pr-4">Estado</th>
            <th className="pb-2">Respuesta</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {registros.map(r => (
            <tr key={r.idBitacoraPublicacion} className="hover:bg-gray-50">
              <td className="py-2 pr-4 whitespace-nowrap text-gray-500">
                {r.fechaIntento ? new Date(r.fechaIntento).toLocaleString('es-MX') : '—'}
              </td>
              <td className="py-2 pr-4 font-medium text-gray-800 max-w-[180px] truncate">
                {r.nombrePersona ?? `ID ${r.idPersonaDesaparecida}`}
              </td>
              <td className="py-2 pr-4">
                <span className="text-xs px-2 py-0.5 rounded-full bg-gray-100 text-gray-600">
                  {r.tipoPublicacion ?? '—'}
                </span>
              </td>
              <td className="py-2 pr-4">
                <span className={`text-xs px-2 py-0.5 rounded-full ${
                  r.estadoPublicacion === 'exitosa'
                    ? 'bg-green-100 text-green-700'
                    : 'bg-red-100 text-red-700'
                }`}>
                  {r.estadoPublicacion ?? '—'}
                </span>
              </td>
              <td className="py-2 text-gray-500 max-w-[200px] truncate text-xs">
                {r.respuestaFacebook ?? '—'}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function TablaGeneral({ registros }) {
  if (registros.length === 0) return <p className="text-center py-8 text-gray-400">Sin registros.</p>
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b text-left text-xs text-gray-500 uppercase tracking-wide">
            <th className="pb-2 pr-4">Fecha</th>
            <th className="pb-2 pr-4">Tipo</th>
            <th className="pb-2 pr-4">Estado</th>
            <th className="pb-2">Descripción</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {registros.map(r => (
            <tr key={r.idBitacoraGeneral} className="hover:bg-gray-50">
              <td className="py-2 pr-4 whitespace-nowrap text-gray-500">
                {r.fechaAccion ? new Date(r.fechaAccion).toLocaleString('es-MX') : '—'}
              </td>
              <td className="py-2 pr-4">
                <span className="text-xs px-2 py-0.5 rounded-full bg-gray-100 text-gray-600">
                  {r.tipoAccion ?? '—'}
                </span>
              </td>
              <td className="py-2 pr-4">
                <span className={`text-xs px-2 py-0.5 rounded-full ${
                  r.estadoAccion === 'exitoso'
                    ? 'bg-green-100 text-green-700'
                    : r.estadoAccion === 'error'
                    ? 'bg-red-100 text-red-700'
                    : 'bg-yellow-100 text-yellow-700'
                }`}>
                  {r.estadoAccion ?? '—'}
                </span>
              </td>
              <td className="py-2 text-gray-500 max-w-[260px] truncate text-xs">
                {r.descripcion ?? '—'}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function useBitacora(url) {
  const [data, setData] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    setLoading(true)
    fetch(url, { headers: authHeader() })
      .then(r => { if (!r.ok) throw new Error(`Error ${r.status}`); return r.json() })
      .then(json => setData(json.data ?? []))
      .catch(e => setError(e.message))
      .finally(() => setLoading(false))
  }, [url])

  return { data, loading, error }
}

export default function BitacoraPage() {
  const [tab, setTab] = useState('publicaciones')
  const [paginaPub, setPaginaPub] = useState(1)
  const [paginaGen, setPaginaGen] = useState(1)

  const pub = useBitacora(API.bitacoraPublicaciones)
  const gen = useBitacora(API.bitacoraGeneral)

  const current = tab === 'publicaciones' ? pub : gen
  const pagina = tab === 'publicaciones' ? paginaPub : paginaGen
  const setPagina = tab === 'publicaciones' ? setPaginaPub : setPaginaGen

  const totalPaginas = Math.ceil(current.data.length / PAGE_SIZE) || 1
  const slice = current.data.slice((pagina - 1) * PAGE_SIZE, pagina * PAGE_SIZE)

  return (
    <div className="flex flex-1">
      <Sidebar />
      <div className="flex-1 max-w-6xl mx-auto px-4 py-6 w-full">
        <h2 className="text-xl font-bold text-gray-800 mb-6">Bitácora</h2>

        <div className="flex gap-1 mb-6 border-b">
          {TABS.map(t => (
            <button
              key={t.id}
              onClick={() => setTab(t.id)}
              className={`px-4 py-2 text-sm font-medium transition-colors cursor-pointer ${
                tab === t.id
                  ? 'border-b-2 text-red-800'
                  : 'text-gray-500 hover:text-gray-800'
              }`}
              style={tab === t.id ? { borderColor: '#8B0000', color: '#8B0000' } : {}}
            >
              {t.label}
            </button>
          ))}
        </div>

        <div className="bg-white rounded-lg border shadow-sm p-4">
          {current.loading && <div className="text-center py-8 text-gray-500">Cargando...</div>}
          {current.error && (
            <div className="text-center py-8 text-red-600 text-sm">
              <p className="font-medium">Error al cargar</p>
              <p className="text-gray-500 mt-1">{current.error}</p>
            </div>
          )}
          {!current.loading && !current.error && (
            <>
              {tab === 'publicaciones'
                ? <TablaPublicaciones registros={slice} />
                : <TablaGeneral registros={slice} />
              }
              {totalPaginas > 1 && (
                <div className="flex items-center justify-center gap-4 pt-4 border-t mt-4">
                  <button
                    onClick={() => setPagina(p => Math.max(1, p - 1))}
                    disabled={pagina === 1}
                    className="px-3 py-1.5 text-sm border rounded disabled:opacity-40 hover:bg-gray-50 cursor-pointer"
                  >
                    ← Anterior
                  </button>
                  <span className="text-sm text-gray-600">
                    Página {pagina} de {totalPaginas}
                  </span>
                  <button
                    onClick={() => setPagina(p => Math.min(totalPaginas, p + 1))}
                    disabled={pagina === totalPaginas}
                    className="px-3 py-1.5 text-sm border rounded disabled:opacity-40 hover:bg-gray-50 cursor-pointer"
                  >
                    Siguiente →
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}
