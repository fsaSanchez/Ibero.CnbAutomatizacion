import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { API } from '../../constants/api'

function Campo({ etiqueta, valor }) {
  if (!valor) return null
  return (
    <div>
      <dt className="text-xs font-medium text-gray-500 uppercase tracking-wide">{etiqueta}</dt>
      <dd className="text-sm text-gray-800 mt-0.5">{valor}</dd>
    </div>
  )
}

export default function PersonaDetallePage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [persona, setPersona] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [fotoPrincipal, setFotoPrincipal] = useState(null)
  const [loadingFoto, setLoadingFoto] = useState(false)

  useEffect(() => {
    fetch(API.personaById(id))
      .then(r => {
        if (!r.ok) throw new Error(`Error ${r.status}`)
        return r.json()
      })
      .then(json => {
        if (json.success) setPersona(json.data)
        else setError(json.message)
      })
      .catch(e => setError(e.message))
      .finally(() => setLoading(false))
  }, [id])

  useEffect(() => {
    if (!persona?.rutaFotoPrincipal) return
    setLoadingFoto(true)
    fetch(API.archivo(persona.rutaFotoPrincipal))
      .then(r => r.json())
      .then(json => {
        if (json.success) {
          const { base64, contentType } = json.data
          setFotoPrincipal(`data:${contentType};base64,${base64}`)
        }
      })
      .catch(() => {})
      .finally(() => setLoadingFoto(false))
  }, [persona?.rutaFotoPrincipal])

  if (loading) return <div className="text-center py-20 text-gray-500">Cargando...</div>
  if (error) return (
    <div className="max-w-2xl mx-auto px-4 py-12 text-center">
      <p className="text-red-600 font-medium">{error}</p>
      <button onClick={() => navigate(-1)} className="mt-4 text-sm text-gray-600 underline cursor-pointer">Volver</button>
    </div>
  )
  if (!persona) return null

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <button
        onClick={() => navigate(-1)}
        className="mb-6 text-sm text-gray-600 hover:text-gray-900 flex items-center gap-1 cursor-pointer"
      >
        ← Volver
      </button>

      <div className="bg-white rounded-xl shadow-sm border overflow-hidden">
        <div className="h-2" style={{ backgroundColor: '#E00034' }} />

        <div className="p-6 sm:flex gap-6">
          <div className="sm:w-48 shrink-0 mb-4 sm:mb-0">
            {fotoPrincipal ? (
              <img
                src={fotoPrincipal}
                alt={persona.nombre}
                className="w-full aspect-square object-cover rounded-lg"
              />
            ) : loadingFoto ? (
              <div className="w-full aspect-square rounded-lg bg-gray-100 flex items-center justify-center">
                <span className="text-sm text-gray-400">Cargando imagen...</span>
              </div>
            ) : (
              <div className="w-full aspect-square rounded-lg bg-gray-100 flex items-center justify-center">
                <span className="text-4xl font-bold text-gray-400">
                  {persona.nombre?.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase()}
                </span>
              </div>
            )}
            {persona.flagPublicadoFacebook && (
              <div className="mt-2 text-center text-xs bg-blue-50 text-blue-700 px-2 py-1 rounded">
                Publicado en Facebook
              </div>
            )}
          </div>

          <div className="flex-1">
            <div className="flex items-start justify-between gap-2 mb-4">
              <div>
                <h1 className="text-2xl font-bold text-gray-900">{persona.nombre}</h1>
                <p className="text-sm text-gray-500 mt-0.5">FUI: {persona.folioUnicoIdentificacion}</p>
              </div>
              <span className={`text-xs px-2 py-1 rounded-full shrink-0 ${
                persona.estadoProcesamiento === 'completo'
                  ? 'bg-green-100 text-green-700'
                  : persona.estadoProcesamiento === 'incompleto'
                  ? 'bg-yellow-100 text-yellow-700'
                  : 'bg-red-100 text-red-700'
              }`}>
                {persona.estadoProcesamiento}
              </span>
            </div>

            <dl className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <Campo etiqueta="Edad actual" valor={persona.edadActual ? `${persona.edadActual} años` : null} />
              <Campo etiqueta="Edad al desaparecer" valor={persona.edadMomentoDesaparicion ? `${persona.edadMomentoDesaparicion} años` : null} />
              <Campo etiqueta="Sexo" valor={persona.sexo} />
              <Campo etiqueta="Género" valor={persona.genero} />
              <Campo etiqueta="Nacionalidad" valor={persona.nacionalidad} />
              <Campo etiqueta="Lugar de nacimiento" valor={persona.lugarNacimiento} />
              <Campo etiqueta="Lugar de los hechos" valor={persona.lugarHechos} />
              <Campo etiqueta="Fecha de los hechos" valor={persona.fechaHechos} />
              <Campo etiqueta="Fecha de percatación" valor={persona.fechaPercate} />
              <Campo etiqueta="Carpeta de investigación" valor={persona.carpetaInvestigacion} />
              <Campo etiqueta="Idioma" valor={persona.idioma} />
              <Campo etiqueta="Discapacidad" valor={persona.discapacidad} />
            </dl>

            {persona.caracteristicasFisicas && (
              <div className="mt-4">
                <dt className="text-xs font-medium text-gray-500 uppercase tracking-wide">Características físicas</dt>
                <dd className="text-sm text-gray-800 mt-0.5">{persona.caracteristicasFisicas}</dd>
              </div>
            )}
            {persona.senasParticulares && (
              <div className="mt-3">
                <dt className="text-xs font-medium text-gray-500 uppercase tracking-wide">Señas particulares</dt>
                <dd className="text-sm text-gray-800 mt-0.5">{persona.senasParticulares}</dd>
              </div>
            )}
            {persona.prendasVestir && (
              <div className="mt-3">
                <dt className="text-xs font-medium text-gray-500 uppercase tracking-wide">Prendas de vestir</dt>
                <dd className="text-sm text-gray-800 mt-0.5">{persona.prendasVestir}</dd>
              </div>
            )}
          </div>
        </div>

        {persona.fotos?.length > 1 && (
          <div className="px-6 pb-6">
            <h3 className="text-sm font-medium text-gray-700 mb-3">Fotos adicionales</h3>
            <div className="flex gap-3 flex-wrap">
              {persona.fotos.map(f => (
                <img
                  key={f.idFotoPersona}
                  src={f.rutaDisco}
                  alt="Foto"
                  className="w-24 h-24 object-cover rounded-lg border"
                />
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
