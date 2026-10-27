using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Tienda_Online.Backend.Data;
using Tienda_Online.Backend.Helpers;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;
using Tienda_Online.Shared.Respuesta;

namespace Tienda_Online.Backend.Clases
{
    public class clsProducto
    {
        private readonly DataContext _context;
        private readonly Cloudinary _cloudinary;


        public clsProducto(DataContext context, IConfiguration config)
        {
            _context = context;
            var account = new Account(
               config["Cloudinary:CloudName"],
               config["Cloudinary:ApiKey"],
               config["Cloudinary:ApiSecret"]
           );
            _cloudinary = new Cloudinary(account);
        }


        public async Task<AccionRespuesta<Producto>> CrearProducto(Producto producto)
        {
            if (string.IsNullOrEmpty(producto.Foto))
            {
                return new AccionRespuesta<Producto>
                {
                    Exitoso = false
                };
            }
          

            // Remover el encabezado 'data:image/jpeg;base64,' si está presente
            var base64Data = producto.Foto;
            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                return new AccionRespuesta<Producto>
                {
                    Exitoso = false
                };
            }

            using (var stream = new MemoryStream(imageBytes))
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(producto.NombreFoto, stream),
                    Transformation = new Transformation().Crop("fill").Gravity("face").Width(500).Height(500)
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return new AccionRespuesta<Producto>
                    {
                        Exitoso = false
                    };
                }
                   

                // Aquí guardamos la URL de la imagen en el atributo Cronograma de TareaParaMadre
                producto.Foto = uploadResult.SecureUrl.ToString();

                // Guardar el modelo actualizado en la base de datos
                _context.Add(producto);
                await _context.SaveChangesAsync();

                // Retornar el modelo guardado con la URL actualizada
                return new AccionRespuesta<Producto>
                {
                    Exitoso = true,
                    Respuesta = producto
                };
            }
        }

        public async Task<AccionRespuesta<Producto>> ActualizarProducto(Producto producto)
        {
            try
            {
                if (string.IsNullOrEmpty(producto.Foto))
                {
                    var foto = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == producto.Id);
                    producto.NombreFoto = foto!.NombreFoto;
                    producto.Foto = foto.Foto;
                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    // Retornar el modelo guardado con la URL actualizada
                    return new AccionRespuesta<Producto>
                    {
                        Exitoso = true,
                        Respuesta = producto
                    };
                }
            }
            catch (Exception e)
            {
                return new AccionRespuesta<Producto>
                {
                    Exitoso = false,
                    Mensaje = e.Message
                };
            }


            // Remover el encabezado 'data:image/jpeg;base64,' si está presente
            var base64Data = producto.Foto;
            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                return new AccionRespuesta<Producto>
                {
                    Exitoso = false
                };
            }

            using (var stream = new MemoryStream(imageBytes))
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(producto.NombreFoto, stream),
                    Transformation = new Transformation().Crop("fill").Gravity("face").Width(500).Height(500)
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return new AccionRespuesta<Producto>
                    {
                        Exitoso = false
                    };
                }


                // Aquí guardamos la URL de la imagen en el atributo Cronograma de TareaParaMadre
                producto.Foto = uploadResult.SecureUrl.ToString();

                // Guardar el modelo actualizado en la base de datos
                _context.Update(producto);
                await _context.SaveChangesAsync();

                // Retornar el modelo guardado con la URL actualizada
                return new AccionRespuesta<Producto>
                {
                    Exitoso = true,
                    Respuesta = producto
                };
            }
        }



        public async Task<AccionRespuesta<IEnumerable<Producto>>> ObtenerListaProducto(PaginacionDTO paginacion)
        {
            var queryable = _context.Productos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(paginacion.Filtro))
            {
                queryable = queryable.Where(x => x.Nombre.ToLower().Contains(paginacion.Filtro.ToLower()));
            }
            return new AccionRespuesta<IEnumerable<Producto>>
            {
                Exitoso = true,
                Respuesta = await queryable.Paginate(paginacion)
                                           .OrderBy(n => n.Nombre)
                                           .ToListAsync()
            };
        }

        public async Task<AccionRespuesta<int>> ObtenerTotalPaginas(PaginacionDTO paginacion)
        {
            var queryable = _context.Productos.AsQueryable();
            double contador = await queryable.CountAsync();
            int totalPaginas = (int)Math.Ceiling(contador / paginacion.NumeroRegistros);
            return new AccionRespuesta<int>
            {
                Exitoso = true,
                Respuesta = totalPaginas
            };
        }

        public async Task<AccionRespuesta<Producto>> BuscarProducto(int id)
        {
            var _producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id);
            if(_producto == null)
            {
                return new AccionRespuesta<Producto>
                {
                    Exitoso = false
                };
            }
            return new AccionRespuesta<Producto>
            {
                Exitoso = true,
                Respuesta = _producto
            };
        }

        public async Task<AccionRespuesta<Producto>> BuscarProductoPorNombre(string nombre)
        {
            var _producto = await _context.Productos.FirstOrDefaultAsync(p => p.Nombre == nombre);
            if (_producto == null)
            {
                return new AccionRespuesta<Producto>
                {
                    Exitoso = false
                };
            }
            return new AccionRespuesta<Producto>
            {
                Exitoso = true,
                Respuesta = _producto
            };
        }

        public async Task<AccionRespuesta<object>> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(o => o.Id == id);
            if(producto == null)
            {
                return new AccionRespuesta<object>
                {
                    Exitoso = false
                };
            }
            _context.Remove(producto);
            await _context.SaveChangesAsync();
            return new AccionRespuesta<object>
            {
                Exitoso = true
            };
        }

        public async Task<AccionRespuesta<Producto>> ActualizarPrecioOferta(Producto producto, double precio)
        {
            try
            {
                producto.Precio = precio;
                _context.Update(producto);
                await _context.SaveChangesAsync();
                return new AccionRespuesta<Producto>
                {
                    Exitoso = true,
                    Respuesta = producto
                };
            }
            catch(Exception e)
            {
                return new AccionRespuesta<Producto>
                {
                    Mensaje = e.Message,
                    Exitoso = false
                };
            }
            
        }
    }
}
