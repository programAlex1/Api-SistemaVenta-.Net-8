using AutoMapper;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;
using System.Runtime.CompilerServices;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace SistemaVenta.BLL.Servicios
{
    public class DashBoardService :IDashBoardService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IGenericRepository<Producto> _productorepository;
        private readonly IMapper _mapper;

        public DashBoardService(IVentaRepository ventaRepository, IGenericRepository<Producto> productorepository, IMapper mapper)
        {
            _ventaRepository = ventaRepository;
            _productorepository = productorepository;
            _mapper = mapper;
        }
        private IQueryable<Venta> retornarVentas(IQueryable<Venta> tablaVenta,int restarCantidadDias)
        {
            DateTime? ultimaFecha = tablaVenta.OrderByDescending(v => v.FechaRegistro).Select(v => v.FechaRegistro).First();

            ultimaFecha = ultimaFecha.Value.AddDays(restarCantidadDias);

            return tablaVenta.Where(v => v.FechaRegistro.Value.Date >= ultimaFecha.Value.Date);
        }

        private async Task<int> TotalVentasUltimaSemana()
        {
            int total = 0;
            IQueryable<Venta> _ventaQuery = await _ventaRepository.Consultar();

            if(_ventaQuery.Count()>0)
            {
                var tablaVenta = retornarVentas(_ventaQuery, -7);
                total = tablaVenta.Count();
            }
            return total;
        }
        private async Task<string> TotalIngresosUltimaSemana()
        {
            decimal resultado = 0;
            IQueryable<Venta> _ventaQuery = await _ventaRepository.Consultar();

            if( _ventaQuery.Count()>0 )
            {
                var tablaVenta = retornarVentas(_ventaQuery, -7);
                resultado = tablaVenta.Select(v => v.Total).Sum(v => v.Value);

            }
            return Convert.ToString(resultado, new CultureInfo("es-PE"));
        }

        private async Task<int> TotalProductos()
        {
            IQueryable<Producto> _productoQuery = await _productorepository.Consultar();    
            int total = _productoQuery.Count();
            return total;
        }
        private async Task<Dictionary<string, int>> VentasUltimaSemana()
        {
            Dictionary<string,int> resultado = new Dictionary<string, int>();

            IQueryable<Venta> _ventaQuery = await _ventaRepository.Consultar(); 

            if(_ventaQuery.Count()> 0 )
            {
                var tablaVenta = retornarVentas(_ventaQuery, -7);

                resultado = tablaVenta
                    .GroupBy(v => v.FechaRegistro.Value.Date).OrderBy(g => g.Key)
                    .Select(g => new {fecha = g.Key.ToString("dd/MM/yyyy"),total = g.Count()})
                    .ToDictionary(keySelector : r => r.fecha,elementSelector: r=> r.total);

            }
            return resultado;
        }



        public async Task<DashboardDTO> Resumen()
        {
           DashboardDTO vmDashBoard = new DashboardDTO();
            try {
                vmDashBoard.TotalVentas = await TotalVentasUltimaSemana();
                vmDashBoard.TotalIngresos= await TotalIngresosUltimaSemana();
                vmDashBoard.TotalProductos = await TotalProductos();

                List<VentaSemanalDTO> listaVenta = new List<VentaSemanalDTO>();
                foreach(KeyValuePair<string,int> item in await VentasUltimaSemana()) 
                {
                    listaVenta.Add(new VentaSemanalDTO()
                    {
                        Fecha=item.Key, 
                        Total= item.Value  
                    });
                }
                vmDashBoard.VentasUltimaSemana = listaVenta;    
            }
            catch
            {
                throw;
            }
            return vmDashBoard;
        }
    }
}
