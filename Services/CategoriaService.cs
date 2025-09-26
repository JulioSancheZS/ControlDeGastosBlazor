using Blazored.LocalStorage;
using BlazorWallet.Model;

namespace BlazorWallet.Services
{
    public class CategoriaService
    {
        private readonly ILocalStorageService _localStorageService;
        private const string StorageKeyTipo = "tipoMovimiento";
        private const string StorageKeyFrecuencia = "frecuencia";
        public CategoriaService(ILocalStorageService localStorageService)
        {
                _localStorageService = localStorageService;
        }

        public async Task InitSeedDataAsync()
        {
            await InitTipoMovimientosAsync();
            await InitFrecuenciasAsync();
        }

        private async Task InitTipoMovimientosAsync()
        {
            var tipos = await _localStorageService.GetItemAsync<List<TipoMovimiento>>(StorageKeyTipo);
            if (tipos == null || tipos.Count == 0)
            {
                tipos = new List<TipoMovimiento>
                {
                    new() { IdTipoMovimiento = Guid.Parse("295e8598-03fb-48ab-96ba-4a573eb96bbe"), Nombre = "Ingreso" },
                    new() { IdTipoMovimiento = Guid.Parse("fdf07f70-9e07-41dc-9c72-ae8ce294859b"), Nombre = "Gasto" },
                    new() { IdTipoMovimiento = Guid.Parse("c9f07564-f022-4a5c-a804-2a56c30347cd"), Nombre = "Ahorro" }
                };

                await _localStorageService.SetItemAsync(StorageKeyTipo, tipos);
            }
        }

        private async Task InitFrecuenciasAsync()
        {
            var frecuencias = await _localStorageService.GetItemAsync<List<Frecuencia>>(StorageKeyFrecuencia);
            if (frecuencias == null || frecuencias.Count == 0)
            {
                frecuencias = new List<Frecuencia>
                {
                    new() { IdFrecuencia = Guid.Parse("14b22f30-fbe3-4831-8b04-264ad7789859"), Nombre = "Quincenal" },
                    new() { IdFrecuencia = Guid.Parse("7e9b9724-b8e6-4c08-8a8c-eed8cb3bb4dc"), Nombre = "Mensual" },
                    new() { IdFrecuencia = Guid.Parse("21074032-2b8c-43a3-a96a-b37f58c31f2f"), Nombre = "Semanal" },
                    new() { IdFrecuencia = Guid.Parse("a68445cb-5b68-4f6d-808c-e84b48e8b73f"), Nombre = "Diario" },
                     new() { IdFrecuencia = Guid.Parse("f0d9ef5a-710c-4d2a-91a9-9add719849da"), Nombre = "Pago Único" }
                };

                await _localStorageService.SetItemAsync(StorageKeyFrecuencia, frecuencias);
            }
        }
        public async Task<List<TipoMovimiento>> GetTipoMovimientos()
           => await _localStorageService.GetItemAsync<List<TipoMovimiento>>(StorageKeyTipo) ?? new();

        public async Task<List<Frecuencia>> GetFrecuencia()
            => await _localStorageService.GetItemAsync<List<Frecuencia>>(StorageKeyFrecuencia) ?? new();
    }
}
