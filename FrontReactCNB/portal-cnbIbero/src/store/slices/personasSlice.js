import { createSlice } from '@reduxjs/toolkit'

export const FILTROS_INICIALES = { folio: '', nombre: '', estado: '', fechaHechos: '', busqueda: '' }

const estadoInicial = {
  datos: [],
  totalRegistros: 0,
  totalPaginas: 1,
  pagina: 1,
  filtrosAplicados: FILTROS_INICIALES,
  cargado: false,
}

const personasSlice = createSlice({
  name: 'personas',
  initialState: estadoInicial,
  reducers: {
    setResultado: (state, action) => {
      const { datos, totalRegistros, pagina, tamanioPagina } = action.payload
      state.datos = datos
      state.totalRegistros = totalRegistros
      state.totalPaginas = Math.max(1, Math.ceil(totalRegistros / tamanioPagina))
      state.pagina = pagina
      state.cargado = true
    },
    setFiltrosAplicados: (state, action) => {
      state.filtrosAplicados = action.payload
    },
  },
})

export const { setResultado, setFiltrosAplicados } = personasSlice.actions
export default personasSlice.reducer
