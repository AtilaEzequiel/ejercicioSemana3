namespace ejercicioSemana3.Middlewares
{
    internal class Response<T>
    {
        public Response()
        {
        }

        public bool Succeeded { get; set; }
        public string Message { get; set; }
    }
}