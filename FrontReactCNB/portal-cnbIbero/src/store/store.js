import { configureStore } from '@reduxjs/toolkit'
import authReducer from './slices/authSlice'
import personasReducer from './slices/personasSlice'
import fotosReducer from './slices/fotosSlice'

export const store = configureStore({
  reducer: { auth: authReducer, personas: personasReducer, fotos: fotosReducer },
})
