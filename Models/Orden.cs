namespace ejercicioSemana3.Models
{
    public class Orden
    {
        public string Action { get; set; }

        public string Symbol { get; set; }
        public string Status { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
