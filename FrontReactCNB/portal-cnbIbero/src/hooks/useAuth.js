import { useSelector, useDispatch } from 'react-redux'
import { setAdminMode, setAuthenticated, setUser } from '../store/slices/authSlice'

export function useAuth() {
  const { isAdmin, isAuthenticated, user } = useSelector(s => s.auth)
  const dispatch = useDispatch()

  return {
    isAdmin,
    isAuthenticated,
    user,
    toggleAdmin: () => dispatch(setAdminMode(!isAdmin)),
    login: (asAdmin = false) => {
      dispatch(setAuthenticated(true))
      dispatch(setAdminMode(asAdmin))
      dispatch(setUser(asAdmin ? { nombre: 'Administrador' } : { nombre: 'Usuario Público' }))
    },
    logout: () => {
      dispatch(setAuthenticated(false))
      dispatch(setAdminMode(false))
      dispatch(setUser(null))
    },
  }
}
