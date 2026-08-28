import { useState } from 'react'
import { API } from '../../constants/api'
import { authHeader } from '../../utilities/auth'
import Sidebar from '../../components/Common/Sidebar'

const REGEX_FUI = /^FI\d{2}-[0-9A-Fa-f]{9}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$/

export default function CeseDifusionPage() {
  const [fui, setFui] = useState('')
  const [procesando, setProcesando] = useState(false)
  const [resultado, setResultado] = useState(null)

  const fuiValido = REGEX_FUI.test(fui.trim())

  async function handleSubmit(e) {
    e.preventDefault()
    if (!fuiValido) return
    setProcesando(true)
    setResultado(null)
    try {
      const res = await fetch(API.ceseDifusion, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...authHeader() },
        body: JSON.stringify({ fui: fui.trim() }),
      })
      setResultado(await res.json())
    } catch {
      setResultado({ success: false, message: 'Error de red. Verifica la conexión con la API.' })
    } finally {
      setProcesando(false)
    }
  }

  function resetear() {
    setFui('')
    setResultado(null)
  }

  return (
    <div className="flex flex-1">
      <Sidebar />
      <div className="flex-1 max-w-2xl mx-auto px-4 py-6 w-full">
        <h2 className="text-xl font-bold font-display text-gray-800 mb-1">Cese de difusión</h2>
        <p className="text-sm text-gray-500 mb-6">
          Da de baja manualmente a una persona localizada: la marca como inactiva y elimina sus
          publicaciones de Facebook. Este proceso normalmente ocurre solo, al recibir un correo de
          "Cese de difusión"; usa esto solo si necesitas forzarlo.
        </p>

        {!resultado ? (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Folio Único de Identificación (FUI)
              </label>
              <input
                type="text"
                value={fui}
                onChange={e => setFui(e.target.value)}
                placeholder="FI26-1B14E1C30-EC2F-4A1D-A74F-273B6C93279A"
                className="w-full border border-gray-300 rounded px-3 py-2 text-sm font-mono focus:outline-none focus:border-red-700"
              />
              {fui && !fuiValido && (
                <p className="text-xs text-red-600 mt-1">Formato de FUI no válido.</p>
              )}
            </div>

            <button
              type="submit"
              disabled={!fuiValido || procesando}
              style={fuiValido && !procesando ? { backgroundColor: '#E00034' } : {}}
              className="px-6 py-2.5 text-sm font-semibold text-white rounded-lg disabled:bg-gray-300 disabled:cursor-not-allowed hover:opacity-90 transition cursor-pointer"
            >
              {procesando ? 'Procesando...' : 'Procesar cese de difusión'}
            </button>

            <p className="text-xs text-gray-400">
              Esta acción es irreversible desde la aplicación: la persona dejará de aparecer en las
              búsquedas públicas y sus publicaciones se eliminarán de Facebook.
            </p>
          </form>
        ) : (
          <div className={`rounded-xl border p-6 ${resultado.success ? 'bg-green-50 border-green-200' : 'bg-red-50 border-red-200'}`}>
            <div className="flex items-start gap-3">
              <span className="text-2xl mt-0.5">{resultado.success ? '✅' : '❌'}</span>
              <div className="flex-1 min-w-0">
                <p className={`font-semibold text-sm leading-snug ${resultado.success ? 'text-green-800' : 'text-red-800'}`}>
                  {resultado.message}
                </p>

                {resultado.success && resultado.data && (
                  <dl className="mt-4 grid grid-cols-2 gap-x-6 gap-y-3 text-xs">
                    <div>
                      <dt className="text-gray-500 uppercase tracking-wide font-medium mb-0.5">Nombre</dt>
                      <dd className="font-semibold text-gray-800">{resultado.data.nombre}</dd>
                    </div>
                    <div>
                      <dt className="text-gray-500 uppercase tracking-wide font-medium mb-0.5">FUI</dt>
                      <dd className="font-semibold text-gray-800">{resultado.data.fui}</dd>
                    </div>
                    <div className="col-span-2">
                      <dt className="text-gray-500 uppercase tracking-wide font-medium mb-0.5">
                        Publicaciones eliminadas de Facebook
                      </dt>
                      <dd className="font-semibold text-gray-800">{resultado.data.publicacionesFacebookEliminadas}</dd>
                    </div>
                  </dl>
                )}
              </div>
            </div>

            <div className="flex gap-3 mt-5">
              <button
                onClick={resetear}
                style={{ borderColor: '#E00034', color: '#E00034' }}
                className="text-xs border rounded-lg px-4 py-2 hover:bg-red-50 cursor-pointer font-medium"
              >
                Procesar otro FUI
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
