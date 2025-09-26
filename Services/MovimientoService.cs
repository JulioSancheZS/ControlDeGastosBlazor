using Blazored.LocalStorage;
using BlazorWallet.Model;
using ClosedXML.Excel;

namespace BlazorWallet.Service
{
    public class MovimientoService
    {
        private readonly ILocalStorageService _localStorageService;
        private const string StorageKeyMovimiento = "movimiento";
        public MovimientoService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public async Task<List<Movimiento>> ListarMovimientosAsync()
        {
            var movimientos = await _localStorageService.GetItemAsync<List<Movimiento>>(StorageKeyMovimiento);
            return movimientos ?? new List<Movimiento>();
        }

        public async Task<List<Movimiento>> ListarPorMesAsync(DateTime fecha)
        {
            var movimientos = await ListarMovimientosAsync();

            return movimientos
                .Where(m =>
                    m.FechaMovimiento.HasValue &&
                    m.FechaMovimiento.Value.Month == fecha.Month &&
                    m.FechaMovimiento.Value.Year == fecha.Year)
                .ToList();
        }


        public async Task GuardarMovimientoAsync(Movimiento movimiento)
        {
            var movimientos = await ListarMovimientosAsync();

            if (movimiento.IdMovimiento == Guid.Empty)
                movimiento.IdMovimiento = Guid.NewGuid();

            movimientos.Add(movimiento);

            await _localStorageService.SetItemAsync(StorageKeyMovimiento, movimientos);
        }

        public async Task EditarMovimientoAsync(Movimiento movimiento)
        {
            var movimientos = await ListarMovimientosAsync();
            var index = movimientos.FindIndex(m => m.IdMovimiento == movimiento.IdMovimiento);
            if (index >= 0)
            {
                movimientos[index] = movimiento;
                await _localStorageService.SetItemAsync(StorageKeyMovimiento, movimientos);
            }
            else
            {
                throw new Exception("Movimiento no encontrado para editar");
            }
        }

        public async Task EliminarMovimientoAsync(Guid idMovimiento)
        {
            var movimientos = await _localStorageService.GetItemAsync<List<Movimiento>>(StorageKeyMovimiento)
                              ?? new List<Movimiento>();

            var movimiento = movimientos.FirstOrDefault(m => m.IdMovimiento == idMovimiento);
            if (movimiento != null)
            {
                movimientos.Remove(movimiento);
                await _localStorageService.SetItemAsync(StorageKeyMovimiento, movimientos);
            }
        }

        public async Task ActualizarMovimientoAsync(Movimiento movimientoActualizado)
        {
            var movimientos = await ListarMovimientosAsync();

            var index = movimientos.FindIndex(m => m.IdMovimiento == movimientoActualizado.IdMovimiento);
            if (index != -1)
            {
                movimientos[index] = movimientoActualizado;
                await _localStorageService.SetItemAsync(StorageKeyMovimiento, movimientos);
            }
        }


        public async Task<byte[]> GenerarExcelMovimientosAsync(
    List<Movimiento> movimientos,
    List<TipoMovimiento> tipos,
    List<Frecuencia> frecuencias)
        {
            using var workbook = new XLWorkbook();

            // Agrupar movimientos por mes
            var movimientosPorMes = movimientos
                .Where(m => m.FechaMovimiento.HasValue)
                .GroupBy(m => new DateTime(m.FechaMovimiento.Value.Year, m.FechaMovimiento.Value.Month, 1))
                .OrderBy(g => g.Key);

            foreach (var grupo in movimientosPorMes)
            {
                var hojaNombre = grupo.Key.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"));
                var hoja = workbook.Worksheets.Add(hojaNombre);

                // Encabezados
                hoja.Cell(1, 1).Value = "Nombre del Movimiento";
                hoja.Cell(1, 2).Value = "Monto (C$)";
                hoja.Cell(1, 3).Value = "Fecha";
                hoja.Cell(1, 4).Value = "Tipo de Movimiento";
                hoja.Cell(1, 5).Value = "Frecuencia";

                int fila = 2;

                foreach (var m in grupo)
                {
                    hoja.Cell(fila, 1).Value = m.NombreMovimiento;
                    hoja.Cell(fila, 2).Value = m.Monto;
                    hoja.Cell(fila, 3).Value = m.FechaMovimiento?.ToString("yyyy-MM-dd");

                    hoja.Cell(fila, 4).Value = tipos.FirstOrDefault(t => t.IdTipoMovimiento == m.IdTipoMovimiento)?.Nombre ?? "Desconocido";
                    hoja.Cell(fila, 5).Value = frecuencias.FirstOrDefault(f => f.IdFrecuencia == m.IdFrecuencia)?.Nombre ?? "Desconocido";

                    fila++;
                }

                // Totales
                decimal totalIngresos = grupo
                    .Where(m => tipos.FirstOrDefault(t => t.IdTipoMovimiento == m.IdTipoMovimiento)?.Nombre == "Ingreso")
                    .Sum(m => m.Monto);

                decimal totalGastos = grupo
                    .Where(m => tipos.FirstOrDefault(t => t.IdTipoMovimiento == m.IdTipoMovimiento)?.Nombre == "Gasto")
                    .Sum(m => m.Monto);

                decimal totalAhorros = grupo
                    .Where(m => tipos.FirstOrDefault(t => t.IdTipoMovimiento == m.IdTipoMovimiento)?.Nombre == "Ahorro")
                    .Sum(m => m.Monto);

                decimal balance = totalIngresos - totalGastos - totalAhorros;

                hoja.Cell(fila + 1, 1).Value = "Totales";
                hoja.Cell(fila + 2, 2).Value = $"Ingresos: C${totalIngresos:N2}";
                hoja.Cell(fila + 3, 2).Value = $"Gastos: C${totalGastos:N2}";
                hoja.Cell(fila + 4, 2).Value = $"Ahorros: C${totalAhorros:N2}";
                hoja.Cell(fila + 5, 2).Value = $"Balance Neto: C${balance:N2}";

                hoja.Row(fila + 1).Style.Font.Bold = true;

                // Autoajustar
                hoja.Columns().AdjustToContents();
                hoja.Row(1).Style.Font.Bold = true;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<List<Movimiento>> ListarPorCicloAsync(DateTime fechaReferencia)
        {
            var (inicio, fin) = ObtenerCiclo(fechaReferencia);

            var movimientos = await ListarMovimientosAsync();

            return movimientos
                .Where(m => m.FechaMovimiento >= inicio && m.FechaMovimiento <= fin)
                .ToList();
        }


        public (DateTime Inicio, DateTime Fin) ObtenerCiclo(DateTime fecha)
        {
            if (fecha.Day >= 15)
            {
                // Segunda quincena (15 → fin de mes)
                var inicio = new DateTime(fecha.Year, fecha.Month, 15);
                var fin = new DateTime(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month));
                return (inicio, fin);
            }
            else
            {
                // Primera quincena (30/31 mes anterior → 14)
                var mesAnterior = fecha.AddMonths(-1);
                var inicio = new DateTime(mesAnterior.Year, mesAnterior.Month, DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month));
                var fin = new DateTime(fecha.Year, fecha.Month, 14);
                return (inicio, fin);
            }
        }


    }
}
