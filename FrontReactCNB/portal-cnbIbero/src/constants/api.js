const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

export const API = {
  personas: `${BASE_URL}/api/personas`,
  personaById: (id) => `${BASE_URL}/api/personas/${id}`,
  publicar: (id) => `${BASE_URL}/api/personas/${id}/publicar`,
  eliminar: (id) => `${BASE_URL}/api/personas/${id}`,
  configuracion: `${BASE_URL}/api/configuracion`,
  configuracionUpdate: (clave) => `${BASE_URL}/api/configuracion/${clave}`,
  bitacoraPublicaciones: `${BASE_URL}/api/bitacora/publicaciones`,
  bitacoraGeneral: `${BASE_URL}/api/bitacora/general`,
}
