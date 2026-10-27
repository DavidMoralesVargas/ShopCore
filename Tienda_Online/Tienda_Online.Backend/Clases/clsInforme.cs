using Microsoft.EntityFrameworkCore;
using Tienda_Online.Backend.Data;
using Tienda_Online.Backend.Helpers;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;
using Tienda_Online.Shared.Respuesta;

namespace Tienda_Online.Backend.Clases
{
    public class clsInforme
    {
        private readonly DataContext _context;
        private readonly clsProducto _producto;

        public clsInforme(DataContext context, clsProducto producto)
        {
            _context = context;
            _producto = producto;
        }

        public async Task<AccionRespuesta<bool>> GuardarInforme(List<CarritoConProductoDTO> carritos)
        {
            try
            {
                foreach (var carrito in carritos)
                {
                    var producto = await _producto.BuscarProductoPorNombre(carrito.NombreProducto);
                    var _informe = await BuscarInformeProducto(producto.Respuesta!.Id);
                    InformeProducto informe = new InformeProducto();
                    if (!_informe.Exitoso)
                    {
                       
                        informe.CodigoProducto = producto.Respuesta.Id;
                        informe.CantidadPedidas = carrito.CantidadProductos;
                        informe.GanaciasTotal = carrito.PrecioTotal;
                        _context.informesProducto.Add(informe);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _informe.Respuesta!.CantidadPedidas += carrito.CantidadProductos;
                        _informe.Respuesta!.GanaciasTotal += carrito.PrecioTotal;
                        _context.Update(_informe.Respuesta);
                        await _context.SaveChangesAsync();
                    }
                }
                return new AccionRespuesta<bool>
                {
                    Respuesta = true,
                    Exitoso = true
                };
            }
            catch (Exception ex)
            {
                return new AccionRespuesta<bool>
                {
                    Exitoso = false,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<AccionRespuesta<InformeProducto>> BuscarInformeProducto(int id)
        {
            try
            {
                var _informe = await _context.informesProducto.FirstOrDefaultAsync(p => p.CodigoProducto == id);
                if (_informe == null)
                {
                    return new AccionRespuesta<InformeProducto>
                    {
                        Exitoso = false
                    };
                }
                return new AccionRespuesta<InformeProducto>
                {
                    Exitoso = true,
                    Respuesta = _informe
                };
            }
            catch(Exception ex)
            {
                return new AccionRespuesta<InformeProducto>
                {
                    Mensaje = ex.Message,
                    Exitoso = false
                };
            }
        }

        public async Task<AccionRespuesta<IEnumerable<InformesDTO>>> ObtenerTotalInformes(PaginacionDTO paginacion)
        {
            try
            {
                var iqueryable = (from P in _context.Set<Producto>()
                                  join I in _context.Set<InformeProducto>()
                                  on P.Id equals I.CodigoProducto
                                  orderby I.GanaciasTotal descending
                                  select new InformesDTO
                                  {
                                      Producto = P.Nombre,
                                      CantidadPedido = I.CantidadPedidas,
                                      Ganacias = I.GanaciasTotal
                                  }
                    ).AsQueryable();
                if (!string.IsNullOrWhiteSpace(paginacion.Filtro))
                {
                    iqueryable = iqueryable.Where(x => x.Producto.ToLower().Contains(paginacion.Filtro.ToLower()));
                }
                return new AccionRespuesta<IEnumerable<InformesDTO>>
                {
                    Exitoso = true,
                    Respuesta = await iqueryable.Paginate(paginacion).ToListAsync()
                };
            }
            catch(Exception ex)
            {
                return new AccionRespuesta<IEnumerable<InformesDTO>>
                {
                    Exitoso = false,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<AccionRespuesta<int>> ObtenerTotalPaginas(PaginacionDTO paginacion)
        {
            try
            {
                var iqueryable = (from P in _context.Set<Producto>()
                                  join I in _context.Set<InformeProducto>()
                                  on P.Id equals I.CodigoProducto
                                  orderby I.GanaciasTotal
                                  select new InformesDTO
                                  {
                                      Producto = P.Nombre,
                                      CantidadPedido = I.CantidadPedidas,
                                      Ganacias = I.GanaciasTotal
                                  }
                    ).AsQueryable();
                double contador = await iqueryable.CountAsync();
                int totalPaginas = (int)Math.Ceiling(contador / paginacion.NumeroRegistros);
                return new AccionRespuesta<int>
                {
                    Exitoso = true,
                    Respuesta = totalPaginas
                };
            }
            catch(Exception ex)
            {
                return new AccionRespuesta<int>
                {
                    Mensaje = ex.Message,
                    Exitoso = false
                };
            }
            
        }
    }
}
