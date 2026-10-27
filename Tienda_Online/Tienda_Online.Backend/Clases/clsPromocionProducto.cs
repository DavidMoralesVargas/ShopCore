using Microsoft.EntityFrameworkCore;
using System.Linq;
using Tienda_Online.Backend.Data;
using Tienda_Online.Backend.Helpers;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;
using Tienda_Online.Shared.Respuesta;

namespace Tienda_Online.Backend.Clases
{
    public class clsPromocionProducto
    {
        private readonly DataContext _context;
        private readonly clsProducto _producto;

        public clsPromocionProducto(DataContext context, clsProducto producto)
        {
            _context = context;
            _producto = producto;
        }

        public async Task<AccionRespuesta<PromocionProducto>> CrearPromocion(PromocionProducto promocion)
        {
            try
            {
                var producto = await _producto.BuscarProducto(promocion.ProductoId);
                if (!producto.Exitoso)
                {
                    return new AccionRespuesta<PromocionProducto>
                    {
                        Mensaje = "El producto no fue encontrado en la base de datos",
                        Exitoso = false
                    };
                }
                promocion.PrecioAntiguo = producto.Respuesta!.Precio;

                _context.Add(promocion);
                await _context.SaveChangesAsync();
                Producto productoActu = producto.Respuesta;
                await _producto.ActualizarPrecioOferta(productoActu, (double)promocion.PrecioOferta);
                return new AccionRespuesta<PromocionProducto>
                {
                    Exitoso = true,
                    Respuesta = promocion
                };
            }
            catch (Exception)
            {
                return new AccionRespuesta<PromocionProducto>
                {
                    Exitoso = false,
                };
            }
        }


        public async Task<AccionRespuesta<PromocionProducto>> EliminarOferta(int id)
        {
            try
            {
                var promocion = await _context.PromocionesProducto.FirstOrDefaultAsync(c => c.Id == id);
                if(promocion == null)
                {
                    return new AccionRespuesta<PromocionProducto>
                    {
                        Mensaje = "Promoción no encontrada en la base de datos",
                        Exitoso = false
                    };
                }
                _context.Remove(promocion);
                await _context.SaveChangesAsync();

                var productoActu = await _producto.BuscarProducto(promocion.ProductoId);
             
                await _producto.ActualizarPrecioOferta(productoActu.Respuesta!, (double)promocion.PrecioAntiguo!);
                return new AccionRespuesta<PromocionProducto>
                {
                    Exitoso = true,
                    Respuesta = promocion
                };
            }
            catch(Exception e)
            {
                return new AccionRespuesta<PromocionProducto>
                {
                    Mensaje = e.Message,
                    Exitoso = false
                };
            }
        }


        public async Task<AccionRespuesta<IEnumerable<OfertaDTO>>> ObtenerListaPromocionProducto(PaginacionDTO paginacion)
        {
            try
            {
                var iqueryable = (from PP in _context.Set<PromocionProducto>()
                                        join P in _context.Set<Producto>()
                                        on PP.ProductoId equals P.Id
                                        orderby P.Nombre
                                        select new OfertaDTO
                                        {
                                            IdPromocion = PP.Id,
                                            IdProducto = P.Id,
                                            FotoProducto = P.Foto,
                                            PrecioAntiguo = (double)PP.PrecioAntiguo!,
                                            PrecioOferta = PP.PrecioOferta,
                                            DescripcionProducto = P.Descripcion
                                        }).AsQueryable();
                if (!string.IsNullOrWhiteSpace(paginacion.Filtro))
                {
                    iqueryable = iqueryable.Where(x => x.DescripcionProducto.ToLower().Contains(paginacion.Filtro.ToLower()));
                }
                return new AccionRespuesta<IEnumerable<OfertaDTO>>
                {
                    Exitoso = true,
                    Respuesta = await iqueryable.Paginate(paginacion).ToListAsync()
                };
            }
            catch (Exception)
            {
                return new AccionRespuesta<IEnumerable<OfertaDTO>>
                {
                    Exitoso = false
                };
            }

        }

        public async Task<AccionRespuesta<int>> ObtenerTotalPaginas(PaginacionDTO paginacion)
        {
            var queryable = (from PP in _context.Set<PromocionProducto>()
                              join P in _context.Set<Producto>()
                              on PP.ProductoId equals P.Id
                              orderby P.Nombre
                              select new OfertaDTO
                              {
                                  IdPromocion = PP.Id,
                                  IdProducto = P.Id,
                                  FotoProducto = P.Foto,
                                  PrecioAntiguo = (double)PP.PrecioAntiguo!,
                                  PrecioOferta = PP.PrecioOferta,
                                  DescripcionProducto = P.Descripcion
                              }).AsQueryable();
            double contador = await queryable.CountAsync();
            int totalPaginas = (int)Math.Ceiling(contador / paginacion.NumeroRegistros);
            return new AccionRespuesta<int>
            {
                Exitoso = true,
                Respuesta = totalPaginas
            };
        }
    }
}
