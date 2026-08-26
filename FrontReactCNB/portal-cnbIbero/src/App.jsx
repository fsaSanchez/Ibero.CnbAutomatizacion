import { useEffect } from 'react'
import { useDispatch } from 'react-redux'
import { Routes, Route, Navigate } from 'react-router-dom'
import PersonasDesaparecidasPage from './pages/PersonasDesaparecidas/PersonasDesaparecidasPage'
import PersonaDetallePage from './pages/PersonasDesaparecidas/PersonaDetallePage'
import ConfiguracionPage from './pages/admin/ConfiguracionPage'
import BitacoraPage from './pages/admin/BitacoraPage'
import CargaPdfPage from './pages/admin/CargaPdfPage'
import LoginPage from './pages/Auth/LoginPage'
import Header from './components/Common/Header'
import Footer from './components/Common/Footer'
import RutaPrivada from './components/Common/RutaPrivada'
import { setSesion } from './store/slices/authSlice'
import { getToken, getUser, tokenValido, limpiarSesionStorage } from './utilities/auth'

export default function App() {
  const dispatch = useDispatch()

  useEffect(() => {
    const token = getToken()
    const user = getUser()
    if (token && user && tokenValido(token)) {
      dispatch(setSesion(user))
    } else if (token) {
      limpiarSesionStorage()
    }
  }, [dispatch])

  return (
    <div className="min-h-screen flex flex-col bg-gray-50">
      <Header />
      <main className="flex-1 flex flex-col">
        <Routes>
          <Route path="/" element={<Navigate to="/personas" replace />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/personas" element={<PersonasDesaparecidasPage />} />
          <Route path="/personas/:id" element={<PersonaDetallePage />} />
          <Route path="/admin/cargar-pdf" element={<RutaPrivada><CargaPdfPage /></RutaPrivada>} />
          <Route path="/admin/configuracion" element={<RutaPrivada><ConfiguracionPage /></RutaPrivada>} />
          <Route path="/admin/bitacora" element={<RutaPrivada><BitacoraPage /></RutaPrivada>} />
          <Route path="*" element={<Navigate to="/personas" replace />} />
        </Routes>
      </main>
      <Footer />
    </div>
  )
}
