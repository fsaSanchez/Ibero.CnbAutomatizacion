import { Routes, Route, Navigate } from 'react-router-dom'
import Home from './pages/Home'
import PersonasDesaparecidasPage from './pages/PersonasDesaparecidas/PersonasDesaparecidasPage'
import PersonaDetallePage from './pages/PersonasDesaparecidas/PersonaDetallePage'
import ConfiguracionPage from './pages/admin/ConfiguracionPage'
import BitacoraPage from './pages/admin/BitacoraPage'
import CargaPdfPage from './pages/admin/CargaPdfPage'
import Header from './components/Common/Header'
import Footer from './components/Common/Footer'

export default function App() {
  return (
    <div className="min-h-screen flex flex-col bg-gray-50">
      <Header />
      <main className="flex-1 flex flex-col">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/personas" element={<PersonasDesaparecidasPage />} />
          <Route path="/personas/:id" element={<PersonaDetallePage />} />
          <Route path="/admin/cargar-pdf" element={<CargaPdfPage />} />
          <Route path="/admin/configuracion" element={<ConfiguracionPage />} />
          <Route path="/admin/bitacora" element={<BitacoraPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
      <Footer />
    </div>
  )
}
