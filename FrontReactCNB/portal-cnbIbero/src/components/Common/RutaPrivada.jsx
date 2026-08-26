import { Navigate } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'

export default function RutaPrivada({ children }) {
  const { isAdmin } = useAuth()
  if (!isAdmin) return <Navigate to="/login" replace />
  return children
}
