import { createSlice } from '@reduxjs/toolkit'

// Caché de fotos ya resueltas (ruta de disco -> data URI base64), para no volver a
// pedirlas a la API cada vez que una tarjeta se vuelve a montar (p.ej. al regresar
// de la pantalla de detalle).
const fotosSlice = createSlice({
  name: 'fotos',
  initialState: {},
  reducers: {
    setFoto: (state, action) => {
      const { ruta, dataUri } = action.payload
      state[ruta] = dataUri
    },
  },
})

export const { setFoto } = fotosSlice.actions
export default fotosSlice.reducer
