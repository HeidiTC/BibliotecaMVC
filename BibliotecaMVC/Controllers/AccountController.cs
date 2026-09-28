using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    // Controlador para manejar el registro, inicio y cierre de sesión de usuarios
    public class AccountController : Controller
    {
        
        private readonly SignInManager<IdentityUser> _signInManager; 
        private readonly UserManager<IdentityUser> _userManager;     

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ===================== LOGIN ====================
        // GET: /Account/Login -> muestra el formulario de login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login -> valida las credenciales y crea la cookie de sesión
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Validamos que el modelo sea correcto (campos requeridos, etc.)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscamos al usuario por nombre o correo electrónico
            var user = await _userManager.FindByNameAsync(model.UserNameOrEmail)
                       ?? await _userManager.FindByEmailAsync(model.UserNameOrEmail);

            if (user != null)
            {
                // Intentamos iniciar sesión con la contraseña proporcionada
                var result = await _signInManager.PasswordSignInAsync(
                    user, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Si hay una URL de retorno válida, redirigimos allí
                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    // Si no hay URL de retorno, redirigimos a la página principal
                    return RedirectToAction("Index", "Home");
                }
            }

            
            ModelState.AddModelError(string.Empty, "Las credenciales introducidas no son correctas.");
            return View(model);
        }

        // ===================== REGISTRO =====================

        // GET: /Account/Register -> muestra el formulario de registro
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register -> crea el usuario en la tabla AspNetUsers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Armamos el usuario con los datos del formulario
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            // CreateAsync guarda el usuario y la contraseña encriptada (hash)
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Iniciamos sesión de una vez con el usuario nuevo
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Si Identity rechazó el registro (usuario repetido, contraseña débil, etc.)
            // mostramos cada error en la vista
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ===================== LOGOUT =====================

        // POST: /Account/Logout -> borra la cookie de sesión
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
