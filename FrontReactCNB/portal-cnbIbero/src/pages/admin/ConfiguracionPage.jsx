import { useState, useEffect } from 'react'
import { API } from '../../constants/api'
import { authHeader } from '../../utilities/auth'
import Sidebar from '../../components/Common/Sidebar'

export default function ConfiguracionPage() {
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [editando, setEditando] = useState({})
  const [guardando, setGuardando] = useState({})
  const [mensajes, setMensajes] = useState({})

  useEffect(() => {
    fetch(API.configuracion, {
      headers: authHeader(),
    })
      .then(r => {
        if (!r.ok) throw new Error(`Error ${r.status}`)
        return r.json()
      })
      .then(json => {
        if (json.success) {
          setItems(json.data ?? [])
          const vals = {}
          ;(json.data ?? []).forEach(c => { vals[c.clave] = c.valor })
          setEditando(vals)
        } else {
          setError(json.message)
        }
      })
      .catch(e => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  async function handleGuardar(clave) {
    setGuardando(prev => ({ ...prev, [clave]: true }))
    try {
      const res = await fetch(API.configuracionUpdate(clave), {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          ...authHeader(),
        },
        body: JSON.stringify({ valor: editando[clave] }),
      })
      const json = await res.json()
      const msg = json.success ? 'Guardado' : (json.message ?? 'Error')
      setMensajes(prev => ({ ...prev, [clave]: { ok: json.success, text: msg } }))
    } catch {
      setMensajes(prev => ({ ...prev, [clave]: { ok: false, text: 'Error de red' } }))
    } finally {
      setGuardando(prev => ({ ...prev, [clave]: false }))
      setTimeout(() => setMensajes(prev => { const n = { ...prev }; delete n[clave]; return n }), 3000)
    }
  }

  return (
    <div className="flex flex-1">
      <Sidebar />
      <div className="flex-1 max-w-4xl mx-auto px-4 py-6 w-full">
        <h2 className="text-xl font-bold font-display text-gray-800 mb-6">Configuración del sistema</h2>

        {loading && <div className="text-center py-12 text-gray-500">Cargando...</div>}

        {error && (
          <div className="bg-red-50 border border-red-200 rounded p-4 text-red-700 text-sm">
            <p className="font-medium">No se pudo cargar la configuración</p>
            <p className="mt-1">{error}</p>
          </div>
        )}

        {!loading && !error && (
          <div className="space-y-3">
            {items.length === 0 && (
              <p className="text-center py-12 text-gray-400">No hay configuraciones registradas.</p>
            )}
            {items.map(item => (
              <div key={item.clave} className="bg-white rounded-lg border shadow-sm p-4">
                <div className="flex items-start gap-4">
                  <div className="flex-1 min-w-0">
                    <label className="block text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                      {item.clave}
                    </label>
                    {item.descripcion && (
                      <p className="text-xs text-gray-400 mb-2">{item.descripcion}</p>
                    )}
                    <input
                      type="text"
                      value={editando[item.clave] ?? ''}
                      onChange={e => setEditando(prev => ({ ...prev, [item.clave]: e.target.value }))}
                      className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:border-red-700"
                    />
                  </div>
                  <div className="flex flex-col items-end gap-1 shrink-0 pt-5">
                    <button
                      onClick={() => handleGuardar(item.clave)}
                      disabled={guardando[item.clave]}
                      style={{ backgroundColor: '#E00034' }}
                      className="px-3 py-1.5 text-xs text-white rounded hover:opacity-90 disabled:opacity-50 cursor-pointer"
                    >
                      {guardando[item.clave] ? 'Guardando...' : 'Guardar'}
                    </button>
                    {mensajes[item.clave] && (
                      <span className={`text-xs ${mensajes[item.clave].ok ? 'text-green-600' : 'text-red-600'}`}>
                        {mensajes[item.clave].text}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
