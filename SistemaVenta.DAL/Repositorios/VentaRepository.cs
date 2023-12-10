using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.Model;
namespace SistemaVenta.DAL.Repositorios
{
    public class VentaRepository : GenericRepository<Venta> , IVentaRepository
    {

        private readonly DbventaContext _dbventaContext;

        public VentaRepository(DbventaContext dbventaContext) : base(dbventaContext) 
        {
            _dbventaContext = dbventaContext;
        }

        public async Task<Venta> Registrar(Venta modelo)
        {
            Venta ventaGenerada = new Venta();

            using (var transaction = _dbventaContext.Database.BeginTransaction())
            {
                try {
                    foreach(DetalleVenta dv in modelo.DetalleVenta)
                    {
                        Producto productoEncontrado = _dbventaContext.Productos.Where(p=> p.IdProducto == dv.IdProducto).First();
                        productoEncontrado.Stock = productoEncontrado.Stock - dv.Cantidad;
                        _dbventaContext.Productos.Update(productoEncontrado);
                    }
                    await _dbventaContext.SaveChangesAsync();

                    NumeroDocumento correlativo = _dbventaContext.NumeroDocumentos.First();
                    correlativo.UltimoNumero = correlativo.UltimoNumero + 1;
                    correlativo.FechaRegistro = DateTime.Now;

                    _dbventaContext.NumeroDocumentos.Update(correlativo);
                    await _dbventaContext.SaveChangesAsync();

                    int CantidadDigitos = 4;
                    string ceros = string.Concat(Enumerable.Repeat("0", CantidadDigitos));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - CantidadDigitos, CantidadDigitos);

                    modelo.NumeroDocumento = numeroVenta;
                    await _dbventaContext.Venta.AddAsync(modelo);
                    await _dbventaContext.SaveChangesAsync();

                    ventaGenerada = modelo;

                    transaction.Commit();
                } catch 
                { 
                    transaction.Rollback();
                    throw;
                }
                return ventaGenerada;
            }
        }
    }
}
