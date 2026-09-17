import { RECAPTCHA_SITE_KEY } from '../constants/api'

let scriptPromise = null

export function cargarRecaptcha() {
  if (window.grecaptcha) return Promise.resolve()
  if (scriptPromise) return scriptPromise

  scriptPromise = new Promise((resolve, reject) => {
    const script = document.createElement('script')
    script.src = `https://www.google.com/recaptcha/api.js?render=${RECAPTCHA_SITE_KEY}`
    script.async = true
    script.defer = true
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('No se pudo cargar reCAPTCHA'))
    document.head.appendChild(script)
  })

  return scriptPromise
}

export async function obtenerTokenRecaptcha(action) {
  await cargarRecaptcha()
  return new Promise((resolve, reject) => {
    window.grecaptcha.ready(() => {
      window.grecaptcha
        .execute(RECAPTCHA_SITE_KEY, { action })
        .then(resolve)
        .catch(reject)
    })
  })
}
