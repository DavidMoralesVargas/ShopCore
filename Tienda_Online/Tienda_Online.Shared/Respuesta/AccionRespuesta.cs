namespace Tienda_Online.Shared.Respuesta
{
    public class AccionRespuesta<T>
    {
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }
        public T? Respuesta { get; set; }

    }
}
