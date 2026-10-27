namespace Tienda_Online.Shared.DTOs
{
    public class PaginacionDTO
    {
        public int Id { get; set; }
        public int Pagina { get; set; } = 1;
        public int NumeroRegistros { get; set; } = 12;
        public string? Filtro { get; set; }
    }
}
