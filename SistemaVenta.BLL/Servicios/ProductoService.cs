using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;

namespace SistemaVenta.BLL.Servicios
{
    public class ProductoService : IProductoService
    {
        private readonly IGenericRepository<Producto> _repository;
        private readonly IMapper _mapper;

        public ProductoService(IGenericRepository<Producto> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ProductoDTO>> Lista()
        {
            try
            {
                var queryProducto = await _repository.Consultar();
                var listaProductos = queryProducto.Include(car => car.IdCategoriaNavigation).ToList();
                return _mapper.Map<List<ProductoDTO>>(listaProductos).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<ProductoDTO> Crear(ProductoDTO modelo)
        {
            try
            {
                var productoCreado = await _repository.Crear(_mapper.Map<Producto>(modelo));

                if (productoCreado.IdProducto == 0)
                    throw new TaskCanceledException("No se pudo realizar la creacion");

                return _mapper.Map<ProductoDTO>(productoCreado);
                
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(ProductoDTO modelo)
        {
            try
            {
                var productoModelo = _mapper.Map<Producto>(modelo);

                var productoEncontrado = await _repository.Obtener(u => u.IdProducto == productoModelo.IdProducto);

                if(productoEncontrado == null)
                    throw new TaskCanceledException("El producto no ha sido encontrado con el id: " + productoModelo.IdProducto);

                productoEncontrado.Nombre=productoModelo.Nombre;
                productoEncontrado.IdCategoria = productoModelo.IdCategoria;
                productoEncontrado.Stock = productoModelo.Stock;
                productoEncontrado.Precio = productoModelo.Precio;
                productoEncontrado.EsActivo = productoModelo.EsActivo;

                bool respuesta = await _repository.Editar(productoEncontrado);

                if (!respuesta)
                    throw new TaskCanceledException("El producto no ha podido ser editado");

                return respuesta;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var productoEncontrado = await _repository.Obtener(u => u.IdProducto == id);
                if (productoEncontrado == null) throw new TaskCanceledException("El producto no ha sido encontrado con el id: " + id);

                bool respuesta = await _repository.Eliminar(productoEncontrado);

                if (!respuesta) throw new TaskCanceledException("El producto no ha podido ser eliminado");

                return respuesta;
            }
            catch
            {
                throw;
            }
        }

       
    }
}
