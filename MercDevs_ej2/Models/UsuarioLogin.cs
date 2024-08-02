using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MercDevs_ej2.Models
{
    public class UsuarioLogin
    {
        public string Correo { get; set; }
        public string Password { get; set; }

        public UsuarioLogin()
        {
            Correo = string.Empty;
            Password = string.Empty;
        }
    }
}