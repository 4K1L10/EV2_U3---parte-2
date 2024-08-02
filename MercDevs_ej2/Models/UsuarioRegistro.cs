namespace MercDevs_ej2.Models
{
    public class UsuarioRegistro
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Password { get; set; }

        public UsuarioRegistro()
        {
            Nombre = string.Empty;
            Apellido = string.Empty;
            Correo = string.Empty;
            Password = string.Empty;
        }
    }
}