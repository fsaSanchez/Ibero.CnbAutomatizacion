import { useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export default function Home() {
  const { login } = useAuth()
  const navigate = useNavigate()

  function iniciar(asAdmin) {
    login(asAdmin)
    navigate('/personas')
  }

  return (
    <div
      className="min-h-screen flex items-center justify-center"
      style={{ background: 'linear-gradient(135deg, #8B0000 0%, #4a0000 100%)' }}
    >
      <div className="bg-white rounded-2xl shadow-2xl p-10 max-w-md w-full mx-4 text-center space-y-6">
        <div className="space-y-2">
          <div
            className="w-16 h-16 rounded-full flex items-center justify-center mx-auto text-2xl font-bold text-white"
            style={{ backgroundColor: '#8B0000' }}
          >
            PUI
          </div>
          <h1 className="text-2xl font-bold text-gray-800 leading-tight">
            Sistema PUI
          </h1>
          <p className="text-gray-500 text-sm">
            Alertas de Personas Desaparecidas
            <br />
            Universidad Iberoamericana
          </p>
        </div>

        <div className="h-px bg-gray-200" />

        <div className="space-y-3">
          <p className="text-sm text-gray-600">Selecciona tu modo de acceso:</p>

          <button
            onClick={() => iniciar(false)}
            className="w-full py-3 px-6 border-2 rounded-lg text-sm font-medium transition-all cursor-pointer hover:bg-red-50"
            style={{ borderColor: '#8B0000', color: '#8B0000' }}
          >
            Iniciar como Usuario Público
          </button>

          <button
            onClick={() => iniciar(true)}
            className="w-full py-3 px-6 rounded-lg text-sm font-medium text-white transition-all cursor-pointer hover:opacity-90"
            style={{ backgroundColor: '#8B0000' }}
          >
            Iniciar como Administrador
          </button>
        </div>

        <p className="text-xs text-gray-400">
          Autenticación simulada — Solo para desarrollo
        </p>
      </div>
    </div>
  )
}
