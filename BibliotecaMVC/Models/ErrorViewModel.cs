namespace BibliotecaMVC.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Semana 15: código HTTP del error (404, 400, 500...). Si viene vacío es una excepción no controlada
        public int? CodigoEstado { get; set; }
    }
}
