export default function ModalConfirmacion({ abierto, mensaje, onConfirmar, onCancelar }) {
  if (!abierto) return null

  return (
    <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-sm w-full p-6 space-y-4">
        <h3 className="text-lg font-semibold text-gray-800">Confirmar acción</h3>
        <p className="text-gray-600 text-sm">{mensaje}</p>
        <div className="flex gap-3 justify-end">
          <button
            onClick={onCancelar}
            className="px-4 py-2 text-sm border border-gray-300 rounded hover:bg-gray-50 transition-colors cursor-pointer"
          >
            Cancelar
          </button>
          <button
            onClick={onConfirmar}
            className="px-4 py-2 text-sm bg-red-700 text-white rounded hover:bg-red-800 transition-colors cursor-pointer"
          >
            Confirmar
          </button>
        </div>
      </div>
    </div>
  )
}
