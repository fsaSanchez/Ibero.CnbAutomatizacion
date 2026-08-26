import { createSlice } from '@reduxjs/toolkit'

const authSlice = createSlice({
  name: 'auth',
  initialState: { isAuthenticated: false, isAdmin: false, user: null },
  reducers: {
    setSesion: (state, action) => {
      state.isAuthenticated = true
      state.isAdmin = true
      state.user = action.payload
    },
    limpiarSesion: (state) => {
      state.isAuthenticated = false
      state.isAdmin = false
      state.user = null
    },
  },
})

export const { setSesion, limpiarSesion } = authSlice.actions
export default authSlice.reducer
