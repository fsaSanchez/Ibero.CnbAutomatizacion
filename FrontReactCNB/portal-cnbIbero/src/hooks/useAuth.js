import { useDispatch, useSelector } from 'react-redux'
import { setSesion, limpiarSesion } from '../store/slices/authSlice'
import { guardarSesion, limpiarSesionStorage } from '../utilities/auth'

export function useAuth() {
  const { isAdmin, isAuthenticated, user } = useSelector(s => s.auth)
  const dispatch = useDispatch()

  return {
    isAdmin,
    isAuthenticated,
    user,
    login: (token, user) => {
      guardarSesion(token, user)
      dispatch(setSesion(user))
    },
    logout: () => {
      limpiarSesionStorage()
      dispatch(limpiarSesion())
    },
  }
}
