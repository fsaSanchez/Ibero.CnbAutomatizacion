const BASE_URL = import.meta.env.VITE_API_URL ?? 'https://localhost:7222'

export const API = {
  personas: `${BASE_URL}/api/personas`,
  personaById: (id) => `${BASE_URL}/api/personas/${id}`,
  publicar: (id) => `${BASE_URL}/api/personas/${id}/publicar`,
  eliminar: (id) => `${BASE_URL}/api/personas/${id}`,
  configuracion: `${BASE_URL}/api/configuracion`,
  configuracionUpdate: (clave) => `${BASE_URL}/api/configuracion/${clave}`,
  bitacoraPublicaciones: `${BASE_URL}/api/bitacora/publicaciones`,
  bitacoraGeneral: `${BASE_URL}/api/bitacora/general`,
  cargaPdfManual: `${BASE_URL}/api/personas/cargar-pdf-manual`,
  listarImagenesPdf: `${BASE_URL}/api/personas/pdf/listar-imagenes`,
  ceseDifusion: `${BASE_URL}/api/personas/cese-difusion`,
  archivo: (ruta) => `${BASE_URL}/api/Personas/archivo?ruta=${encodeURIComponent(ruta)}`,
}
