import { useState, useRef, useCallback } from 'react'
import { API } from '../../constants/api'
import { authHeader } from '../../utilities/auth'
import Sidebar from '../../components/Common/Sidebar'

const MAX_BYTES = 10 * 1024 * 1024 // 10 MB

function EstadoBadge({ estado }) {
  const cls = estado === 'completo'
    ? 'bg-green-100 text-green-700'
    : 'bg-yellow-100 text-yellow-700'
  return <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}>{estado}</span>
}

export default function CargaPdfPage() {
  const [archivo, setArchivo] = useState(null)
  const [dragOver, setDragOver] = useState(false)
  const [procesando, setProcesando] = useState(false)
  const [resultado, setResultado] = useState(null)
  const [errorLocal, setErrorLocal] = useState(null)
  const inputRef = useRef(null)

  const [mostrarProbador, setMostrarProbador] = useState(false)
  const [archivoIndice, setArchivoIndice] = useState(null)
  const [analizando, setAnalizando] = useState(false)
  const [imagenesIndice, setImagenesIndice] = useState(null)
  const [errorIndice, setErrorIndice] = useState(null)
  const inputIndiceRef = useRef(null)

  function validar(file) {
    if (!file) return 'No se seleccionó ningún archivo.'
    if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf'))
      return 'Solo se aceptan archivos PDF.'
    if (file.size > MAX_BYTES)
      return 'El archivo supera el límite de 10 MB.'
    return null
  }

  function seleccionar(file) {
    const err = validar(file)
    if (err) { setErrorLocal(err); setArchivo(null); return }
    setErrorLocal(null)
    setResultado(null)
    setArchivo(file)
  }

  const onDrop = useCallback(e => {
    e.preventDefault()
    setDragOver(false)
    seleccionar(e.dataTransfer.files[0])
  }, [])

  async function handleProcesar() {
    if (!archivo) return
    setProcesando(true)
    setResultado(null)
    try {
      const form = new FormData()
      form.append('archivo', archivo)
      const res = await fetch(API.cargaPdfManual, { method: 'POST', headers: authHeader(), body: form })
      setResultado(await res.json())
    } catch {
      setResultado({ success: false, message: 'Error de red. Verifica la conexión con la API.' })
    } finally {
      setProcesando(false)
    }
  }

  function resetear() {
    setArchivo(null)
    setResultado(null)
    setErrorLocal(null)
    if (inputRef.current) inputRef.current.value = ''
  }

  function seleccionarIndice(file) {
    const err = validar(file)
    if (err) { setErrorIndice(err); setArchivoIndice(null); return }
    setErrorIndice(null)
    setImagenesIndice(null)
    setArchivoIndice(file)
  }

  async function handleAnalizarIndice() {
    if (!archivoIndice) return
    setAnalizando(true)
    setImagenesIndice(null)
    setErrorIndice(null)
    try {
      const form = new FormData()
      form.append('archivo', archivoIndice)
      const res = await fetch(API.listarImagenesPdf, { method: 'POST', headers: authHeader(), body: form })
      const data = await res.json()
      if (data.success) {
        setImagenesIndice(data.data ?? [])
      } else {
        setErrorIndice(data.message ?? 'No se pudo analizar el PDF.')
      }
    } catch {
      setErrorIndice('Error de red. Verifica la conexión con la API.')
    } finally {
      setAnalizando(false)
    }
  }

  function resetearIndice() {
    setArchivoIndice(null)
    setImagenesIndice(null)
    setErrorIndice(null)
    if (inputIndiceRef.current) inputIndiceRef.current.value = ''
  }

  return (
    <div className="flex flex-1">
      <Sidebar />
      <div className="flex-1 max-w-2xl mx-auto px-4 py-6 w-full">
        <h2 className="text-xl font-bold text-gray-800 mb-1">Cargar PDF manualmente</h2>
        <p className="text-sm text-gray-500 mb-6">
          Sube una ficha oficial de búsqueda en PDF para registrarla en el sistema sin necesidad de correo electrónico.
        </p>

        {!resultado ? (
          <>
            {/* Zona drag & drop */}
            <div
              onDragOver={e => { e.preventDefault(); setDragOver(true) }}
              onDragLeave={() => setDragOver(false)}
              onDrop={onDrop}
              onClick={() => inputRef.current?.click()}
              className={`border-2 border-dashed rounded-xl p-12 text-center cursor-pointer transition-colors select-none ${
                dragOver
                  ? 'bg-red-50'
                  : archivo
                  ? 'border-green-400 bg-green-50'
                  : 'border-gray-300 hover:border-gray-400 bg-gray-50'
              }`}
              style={dragOver ? { borderColor: '#8B0000' } : {}}
            >
              <input
                ref={inputRef}
                type="file"
                accept=".pdf,application/pdf"
                className="hidden"
                onChange={e => seleccionar(e.target.files?.[0])}
              />

              {archivo ? (
                <>
                  <div className="text-4xl mb-3">📄</div>
                  <p className="font-semibold text-gray-800 text-sm">{archivo.name}</p>
                  <p className="text-xs text-gray-400 mt-1">{(archivo.size / 1024).toFixed(0)} KB</p>
                  <p className="text-xs text-green-600 mt-2 font-medium">Archivo listo para procesar</p>
                </>
              ) : (
                <>
                  <div className="text-5xl mb-4 text-gray-200">↑</div>
                  <p className="text-sm font-medium text-gray-600">
                    Arrastra el PDF aquí o{' '}
                    <span style={{ color: '#8B0000' }} className="font-semibold">haz clic para seleccionar</span>
                  </p>
                  <p className="text-xs text-gray-400 mt-2">Solo archivos PDF · Máximo 10 MB</p>
                </>
              )}
            </div>

            {errorLocal && (
              <p className="mt-3 text-sm text-red-600 font-medium">{errorLocal}</p>
            )}

            <div className="flex gap-3 mt-5">
              <button
                onClick={handleProcesar}
                disabled={!archivo || procesando}
                style={archivo && !procesando ? { backgroundColor: '#8B0000' } : {}}
                className="px-6 py-2.5 text-sm font-semibold text-white rounded-lg disabled:bg-gray-300 disabled:cursor-not-allowed hover:opacity-90 transition cursor-pointer"
              >
                {procesando ? 'Procesando...' : 'Procesar PDF'}
              </button>
              {archivo && !procesando && (
                <button
                  onClick={resetear}
                  className="px-4 py-2.5 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 cursor-pointer text-gray-600"
                >
                  Cancelar
                </button>
              )}
            </div>

            {procesando && (
              <div className="mt-6 flex items-center gap-3 text-sm text-gray-500">
                <div
                  className="w-5 h-5 border-2 border-gray-200 border-t-red-800 rounded-full animate-spin shrink-0"
                  style={{ borderTopColor: '#8B0000' }}
                />
                Extrayendo datos del PDF… esto puede tardar unos segundos si se usa el fallback de IA.
              </div>
            )}
          </>
        ) : (
          /* Panel de resultado */
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
                      <dd className="font-semibold text-gray-800">{resultado.data.folioUnicoIdentificacion}</dd>
                    </div>
                    <div>
                      <dt className="text-gray-500 uppercase tracking-wide font-medium mb-0.5">Estado extracción</dt>
                      <dd className="mt-0.5"><EstadoBadge estado={resultado.data.estado} /></dd>
                    </div>
                    <div>
                      <dt className="text-gray-500 uppercase tracking-wide font-medium mb-0.5">ID en sistema</dt>
                      <dd className="font-semibold text-gray-800">#{resultado.data.idPersonaDesaparecida}</dd>
                    </div>
                    {resultado.data.camposIncompletos?.length > 0 && (
                      <div className="col-span-2">
                        <dt className="text-gray-500 uppercase tracking-wide font-medium mb-0.5">Campos incompletos</dt>
                        <dd className="text-yellow-700">{resultado.data.camposIncompletos.join(', ')}</dd>
                      </div>
                    )}
                  </dl>
                )}
              </div>
            </div>

            <div className="flex gap-3 mt-5">
              <button
                onClick={resetear}
                style={{ borderColor: '#8B0000', color: '#8B0000' }}
                className="text-xs border rounded-lg px-4 py-2 hover:bg-red-50 cursor-pointer font-medium"
              >
                Cargar otro PDF
              </button>
              {resultado.success && resultado.data && (
                <a
                  href={`/personas/${resultado.data.idPersonaDesaparecida}`}
                  style={{ backgroundColor: '#8B0000' }}
                  className="text-xs text-white rounded-lg px-4 py-2 hover:opacity-90 cursor-pointer font-medium inline-flex items-center"
                >
                  Ver ficha →
                </a>
              )}
            </div>
          </div>
        )}

        {/* Apartado: Probar índice de foto */}
        <div className="mt-10 border-t border-gray-200 pt-6">
          <button
            onClick={() => setMostrarProbador(v => !v)}
            className="flex items-center gap-2 text-sm font-semibold text-gray-700 hover:text-gray-900 cursor-pointer"
          >
            <span className={`inline-block transition-transform ${mostrarProbador ? 'rotate-90' : ''}`}>▶</span>
            Probar índice de foto
          </button>
          <p className="text-xs text-gray-500 mt-1 ml-5">
            Sube un PDF de muestra y revisa todas sus imágenes con su página e índice, para saber
            qué valor configurar en <strong>indice_foto</strong> cuando cambie el formato de la ficha.
          </p>

          {mostrarProbador && (
            <div className="mt-4 ml-5">
              <div className="flex flex-wrap items-center gap-3">
                <input
                  ref={inputIndiceRef}
                  type="file"
                  accept=".pdf,application/pdf"
                  onChange={e => seleccionarIndice(e.target.files?.[0])}
                  className="text-xs text-gray-600 file:mr-3 file:py-2 file:px-3 file:rounded-lg file:border-0 file:text-xs file:font-semibold file:bg-gray-100 file:text-gray-700 hover:file:bg-gray-200 file:cursor-pointer cursor-pointer"
                />
                <button
                  onClick={handleAnalizarIndice}
                  disabled={!archivoIndice || analizando}
                  style={archivoIndice && !analizando ? { backgroundColor: '#8B0000' } : {}}
                  className="px-4 py-2 text-xs font-semibold text-white rounded-lg disabled:bg-gray-300 disabled:cursor-not-allowed hover:opacity-90 transition cursor-pointer"
                >
                  {analizando ? 'Analizando...' : 'Analizar PDF'}
                </button>
                {(archivoIndice || imagenesIndice) && !analizando && (
                  <button
                    onClick={resetearIndice}
                    className="px-3 py-2 text-xs border border-gray-300 rounded-lg hover:bg-gray-50 cursor-pointer text-gray-600"
                  >
                    Limpiar
                  </button>
                )}
              </div>

              {errorIndice && (
                <p className="mt-3 text-xs text-red-600 font-medium">{errorIndice}</p>
              )}

              {imagenesIndice && imagenesIndice.length > 0 && (
                <div className="mt-4 grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
                  {imagenesIndice.map((img, i) => (
                    <div key={i} className="border border-gray-200 rounded-lg overflow-hidden bg-white">
                      <img
                        src={`data:${img.tipoContenido};base64,${img.imagenBase64}`}
                        alt={`Página ${img.pagina} · Índice ${img.indice}`}
                        className="w-full h-32 object-contain bg-gray-50"
                      />
                      <div className="p-2 text-center">
                        <p className="text-xs font-semibold text-gray-800">
                          Página {img.pagina} · Índice {img.indice}
                        </p>
                        <p className="text-[10px] text-gray-400">{(img.tamanoBytes / 1024).toFixed(0)} KB</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {imagenesIndice && imagenesIndice.length === 0 && (
                <p className="mt-3 text-xs text-gray-500">No se encontraron imágenes en este PDF.</p>
              )}

              {imagenesIndice && imagenesIndice.length > 0 && (
                <p className="mt-3 text-xs text-gray-500">
                  Identifica la foto correcta y actualiza el valor <strong>indice_foto</strong> en{' '}
                  <a href="/admin/configuracion" style={{ color: '#8B0000' }} className="font-semibold hover:underline">
                    Configuración
                  </a>.
                </p>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
