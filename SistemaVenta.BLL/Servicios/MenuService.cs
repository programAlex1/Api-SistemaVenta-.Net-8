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

namespace SistemaVenta.BLL.Servicios
{
    public class MenuService : IMenuService
    {
        private readonly IGenericRepository<Usuario> _usuariorepository;
        private readonly IGenericRepository<Menu> _menurepository;
        private readonly IGenericRepository<MenuRol> _menuRolrepository;
        private readonly IMapper _mapper;

        public MenuService(IGenericRepository<Usuario> usuariorepository, IGenericRepository<Menu> menurepository, IGenericRepository<MenuRol> menuRolrepository, IMapper mapper)
        {
            _usuariorepository = usuariorepository;
            _menurepository = menurepository;
            _menuRolrepository = menuRolrepository;
            _mapper = mapper;
        }

        public async Task<List<MenuDTO>> Lista(int idUsuario)
        {
            IQueryable<Usuario> tbUsuario = await _usuariorepository.Consultar(u => u.IdUsuario == idUsuario);
            IQueryable<MenuRol> tbMenuRol = await _menuRolrepository.Consultar();
            IQueryable<Menu> tbMenu = await _menurepository.Consultar();

            try
            {
                IQueryable<Menu> tbResultado = (from u in tbUsuario join mr in tbMenuRol on u.IdRol equals mr.IdRol join m in tbMenu on mr.IdMenu equals m.IdMenu
                                                select m).AsQueryable();   
                
                var listaMenus = tbResultado.ToList();  
                return _mapper.Map<List<MenuDTO>>(listaMenus);  
            }
            catch
            {
                throw;
            }
        }
    }
}
