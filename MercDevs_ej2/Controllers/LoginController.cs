using MercDevs_ej2.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MercDevs_ej2.Controllers
{
    public class LoginController : Controller
    {
        private readonly MercydevsEjercicio2Context _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public LoginController(MercydevsEjercicio2Context context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();

        }

        [HttpGet]
        public IActionResult Ingresar()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ingresar(UsuarioLogin usuarioLogin)
        {
            var usuario = await _context.Usuarios.
                FirstOrDefaultAsync(u =>
                    u.Correo == usuarioLogin.Correo);

            if (usuario == null)

            {
                ViewData["Mensaje"] = "Nombre de usuario o contraseña no Coinciden";
                return View();
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Surname, usuario.Apellido), // Verifica que esta línea no tenga errores
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim("Id", usuario.IdUsuario.ToString())
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    properties
                );

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(UsuarioRegistro usuarioRegistro)
        {
            if (ModelState.IsValid)
            {
                var nuevoUsuario = new Usuario
                {
                    Nombre = usuarioRegistro.Nombre,
                    Apellido = usuarioRegistro.Apellido,
                    Correo = usuarioRegistro.Correo,
                };

                nuevoUsuario.Password = _passwordHasher.HashPassword(nuevoUsuario, usuarioRegistro.Password);

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                return RedirectToAction("Ingresar");
            }

            return View(usuarioRegistro);
        }
    }
}