import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import { API } from '../../constants/api'
import { setFoto } from '../../store/slices/fotosSlice'
import ModalConfirmacion from './ModalConfirmacion'

function Iniciales({ nombre }) {
  const partes = nombre?.split(' ') ?? []
  const iniciales = partes.slice(0, 2).map(p => p[0]).join('').toUpperCase()
  return (
    <div className="w-full h-36 flex items-center justify-center bg-gray-100">
      <span className="text-4xl font-bold text-gray-400">{iniciales || '?'}</span>
    </div>
  )
}

export default function TarjetaPersona({ persona, isAdmin, onEliminar, onPublicar }) {
  const navigate = useNavigate()
  const dispatch = useDispatch()
  const [modalEliminar, setModalEliminar] = useState(false)
  const ruta = persona.rutaFotoPrincipal
  const fotoPrincipal = useSelector(s => (ruta ? s.fotos[ruta] : undefined))

  useEffect(() => {
    // Ya está en caché (p.ej. se regresó de la pantalla de detalle): no se vuelve a pedir.
    if (!ruta || fotoPrincipal) return
    let cancelado = false
    fetch(API.archivo(ruta))
      .then(r => r.json())
      .then(json => {
        if (cancelado || !json.success) return
        const { base64, contentType } = json.data
        dispatch(setFoto({ ruta, dataUri: `data:${contentType};base64,${base64}` }))
      })
      .catch(() => {})
    return () => { cancelado = true }
  }, [ruta, fotoPrincipal, dispatch])

  return (
    <>
      <div
        className="bg-white rounded-lg shadow-sm border overflow-hidden flex flex-col"
        style={{ height: '280px', borderTop: '3px solid #E00034' }}
      >
        <div className="h-36 overflow-hidden shrink-0 bg-gray-100">
          {fotoPrincipal ? (
            <img
              src={fotoPrincipal}
              alt={persona.nombre }
              className="w-full h-full object-contain"
              onError={e => { e.target.style.display = 'none'; e.target.nextSibling.style.display = 'flex' }}
            />
          ) : null}
          <Iniciales nombre={persona.nombre +""} />
        </div>

        <div className="p-3 flex flex-col flex-1 overflow-hidden">
          <p className="font-semibold text-gray-800 text-sm truncate leading-tight">{persona.nombre}</p>
          <p className="text-gray-500 text-xs mt-0.5 truncate">
            {persona.edadActual ? `${persona.edadActual} años` : 'Edad desconocida'}
            {persona.lugarHechos ? ` · ${persona.lugarHechos}` : ''}
          </p>

          {isAdmin && (
            <div className="flex gap-1 mt-1 flex-wrap">
              <span className={`text-xs px-1.5 py-0.5 rounded-full ${
                persona.estadoProcesamiento === 'completo'
                  ? 'bg-green-100 text-green-700'
                  : persona.estadoProcesamiento === 'incompleto'
                  ? 'bg-yellow-100 text-yellow-700'
                  : 'bg-red-100 text-red-700'
              }`}>
                {persona.estadoProcesamiento}
              </span>
              {persona.flagPublicadoFacebook && (
                <span className="text-xs px-1.5 py-0.5 rounded-full bg-blue-100 text-blue-700">
                  Facebook ✓
                </span>
              )}
            </div>
          )}

          <div className="flex gap-1 mt-auto flex-wrap">
            <button
              onClick={() => navigate(`/personas/${persona.idPersonaDesaparecida}`)}
              style={{ backgroundColor: '#E00034' }}
              className="text-xs text-white px-2 py-1 rounded hover:opacity-90 transition-opacity cursor-pointer"
            >
              Ver más
            </button>
            {isAdmin && (
              <>
                {!persona.flagPublicadoFacebook && (
                  <button
                    onClick={() => onPublicar(persona.idPersonaDesaparecida)}
                    className="text-xs bg-blue-600 text-white px-2 py-1 rounded hover:bg-blue-700 transition-colors cursor-pointer"
                  >
                    Publicar
                  </button>
                )}
                <button
                  onClick={() => setModalEliminar(true)}
                  className="text-xs bg-gray-200 text-gray-700 px-2 py-1 rounded hover:bg-gray-300 transition-colors cursor-pointer"
                >
                  Eliminar
                </button>
              </>
            )}
          </div>
        </div>
      </div>

      <ModalConfirmacion
        abierto={modalEliminar}
        mensaje={`¿Eliminar a "${persona.nombre}"? Esta acción es permanente.`}
        onConfirmar={() => { setModalEliminar(false); onEliminar(persona.idPersonaDesaparecida) }}
        onCancelar={() => setModalEliminar(false)}
      />
    </>
  )
}
