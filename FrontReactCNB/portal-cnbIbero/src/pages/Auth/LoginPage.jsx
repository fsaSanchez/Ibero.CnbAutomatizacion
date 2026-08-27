import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'
import IberoWordmark from '../../components/Common/IberoWordmark'

const LOGIN_URL = 'https://solicitudesti.ibero.mx/back/api/Auth/LoginExterno'
// Valores asignados a "Sistema PUI" en el sistema institucional (solicitudesti.ibero.mx)
const ID_DET_APP = 426
const PROFILE_ID_ADMIN = 468
const ID_TIPO_EMPLEADO = 24

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [account, setAccount] = useState('')
  const [password, setPassword] = useState('')
  const [mostrarPassword, setMostrarPassword] = useState(false)
  const [enviando, setEnviando] = useState(false)
  const [error, setError] = useState(null)

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)
    setEnviando(true)
    try {
      const res = await fetch(LOGIN_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          account,
          digit: '0',
          password,
          idDetApp: ID_DET_APP,
          profileId: PROFILE_ID_ADMIN,
          idTipoUsuario: ID_TIPO_EMPLEADO,
        }),
      })
      const json = await res.json()

      if (json.success === 1 && json.data?.token) {
        login(json.data.token, {
          nombre: json.data.userName,
          email: json.data.email,
          perfil: json.data.profile,
        })
        navigate('/personas')
      } else {
        setError(json.message ?? 'No se pudo iniciar sesión.')
      }
    } catch {
      setError('Error de red. Verifica tu conexión.')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div
      className="min-h-screen flex flex-col items-center justify-center flex-1 gap-6 py-10 px-4"
      style={{ background: 'linear-gradient(135deg, #E00034 0%, #8a0021 100%)' }}
    >
      <img
        src="/brand/rubrica-ibero-blanco.png"
        alt="Universidad Iberoamericana Tijuana"
        className="h-44 w-auto drop-shadow-lg"
      />

      <div className="bg-white rounded-2xl shadow-2xl p-10 max-w-md w-full">
        <div className="text-center space-y-2 mb-6">
          <IberoWordmark className="h-12 w-auto mx-auto text-gray-800" />
          <h1 className="text-2xl font-bold font-display text-gray-800">Iniciar sesión</h1>
          <p className="text-gray-500 text-sm">Acceso de administrador — Sistema PUI</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
              Número de empleado
            </label>
            <input
              type="text"
              value={account}
              onChange={e => setAccount(e.target.value)}
              required
              className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:border-red-700"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
              Contraseña
            </label>
            <div className="relative">
              <input
                type={mostrarPassword ? 'text' : 'password'}
                value={password}
                onChange={e => setPassword(e.target.value)}
                required
                className="w-full border border-gray-300 rounded px-3 py-2 text-sm pr-14 focus:outline-none focus:border-red-700"
              />
              <button
                type="button"
                onClick={() => setMostrarPassword(v => !v)}
                className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 cursor-pointer text-xs"
              >
                {mostrarPassword ? 'Ocultar' : 'Ver'}
              </button>
            </div>
          </div>

          {error && <p className="text-sm text-red-600 font-medium">{error}</p>}

          <button
            type="submit"
            disabled={enviando}
            style={{ backgroundColor: '#E00034' }}
            className="w-full py-2.5 rounded-lg text-sm font-semibold text-white hover:opacity-90 disabled:opacity-50 cursor-pointer transition"
          >
            {enviando ? 'Ingresando...' : 'Ingresar'}
          </button>

          <button
            type="button"
            onClick={() => navigate('/personas')}
            className="w-full text-xs text-gray-500 hover:text-gray-700 cursor-pointer"
          >
            ← Volver sin iniciar sesión
          </button>
        </form>
      </div>
    </div>
  )
}
