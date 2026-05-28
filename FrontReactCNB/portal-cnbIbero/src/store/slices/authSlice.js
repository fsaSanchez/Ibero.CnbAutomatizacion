import { createSlice } from '@reduxjs/toolkit'

const authSlice = createSlice({
  name: 'auth',
  initialState: { isAdmin: false, isAuthenticated: false, user: null },
  reducers: {
    setAdminMode: (state, action) => { state.isAdmin = action.payload },
    setAuthenticated: (state, action) => { state.isAuthenticated = action.payload },
    setUser: (state, action) => { state.user = action.payload },
  },
})

export const { setAdminMode, setAuthenticated, setUser } = authSlice.actions
export default authSlice.reducer
