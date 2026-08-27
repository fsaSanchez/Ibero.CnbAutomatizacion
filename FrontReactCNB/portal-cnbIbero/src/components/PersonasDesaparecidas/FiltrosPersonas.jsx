export default function FiltrosPersonas({ filtros, onChange, onBuscar, onLimpiar }) {
  function handleSubmit(e) {
    e.preventDefault()
    onBuscar()
  }

  return (
    <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-sm border p-4 mb-6">
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-3">
        <input
          type="text"
          placeholder="FUI / Folio"
          value={filtros.folio ?? ''}
          onChange={e => onChange('folio', e.target.value)}
          className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:border-red-700"
        />
        <input
          type="text"
          placeholder="Nombre"
          value={filtros.nombre ?? ''}
          onChange={e => onChange('nombre', e.target.value)}
          className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:border-red-700"
        />
        <select
          value={filtros.estado ?? ''}
          onChange={e => onChange('estado', e.target.value)}
          className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:border-red-700 text-gray-600"
        >
          <option value="">Estado (todos)</option>
          <option value="completo">Completo</option>
          <option value="incompleto">Incompleto</option>
          <option value="error">Error</option>
        </select>
        <input
          type="date"
          value={filtros.fechaHechos ?? ''}
          onChange={e => onChange('fechaHechos', e.target.value)}
          className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:border-red-700 text-gray-600"
        />
        <input
          type="text"
          placeholder="Búsqueda libre"
          value={filtros.busqueda ?? ''}
          onChange={e => onChange('busqueda', e.target.value)}
          className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:border-red-700"
        />
      </div>
      <div className="flex gap-2 mt-3 justify-end">
        <button
          type="button"
          onClick={onLimpiar}
          className="px-4 py-2 text-sm border border-gray-300 rounded hover:bg-gray-50 transition-colors cursor-pointer"
        >
          Limpiar
        </button>
        <button
          type="submit"
          style={{ backgroundColor: '#E00034' }}
          className="px-4 py-2 text-sm text-white rounded hover:opacity-90 transition-opacity cursor-pointer"
        >
          Buscar
        </button>
      </div>
    </form>
  )
}
