import IberoWordmark from './IberoWordmark'

export default function Footer() {
  return (
    <footer className="bg-gray-800 text-gray-300 mt-auto">
      <div className="max-w-7xl mx-auto px-4 py-6 text-center text-sm space-y-2">
        <IberoWordmark className="h-10 w-auto mx-auto text-white/90" />
        <p>
          Para reportar información sobre personas desaparecidas, comuníquese al número de emergencias{' '}
          <strong className="text-white">800-IBERO-00</strong> o al correo{' '}
          <strong className="text-white">pui@ibero.mx</strong>
        </p>
        <p className="text-gray-500 text-xs mt-2">
          © {new Date().getFullYear()} Universidad Iberoamericana — Sistema PUI
        </p>
      </div>
    </footer>
  )
}
