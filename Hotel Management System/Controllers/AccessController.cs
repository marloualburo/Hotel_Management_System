using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hotel_Management_System.Models;
using System.Threading.Tasks;
using System.Linq;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Hotel_Management_System.Controllers
{
    public class AccessController : Controller
    {
        private readonly HotelManagementDbContext _context;
        private readonly ILogger<AccessController> _logger;

        public AccessController(HotelManagementDbContext context, ILogger<AccessController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Access/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Access/Register
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if the email is already registered
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(user);
                }

                // Hash the password using BCrypt
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                // Add the user to the database
                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(user);
        }

        // GET: Access/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            // If the user is already authenticated, redirect to the home page
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: Access/Login
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(VMLogin modelLogin)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = _context.Users.FirstOrDefault(u => u.Email == modelLogin.Email);

                if (user != null)
                {
                    // Verify the password
                    var isPasswordValid = BCrypt.Net.BCrypt.Verify(modelLogin.Password, user.PasswordHash);
                    if (isPasswordValid)
                    {
                        // Create claims for the authenticated user
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Email),
                            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                            new Claim("OtherProperties", "ExampleRole")
                        };

                        // Create the identity and principal
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            AllowRefresh = true,
                            IsPersistent = modelLogin.RememberMe
                        };

                        // Sign in the user
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Log invalid password
                        _logger.LogWarning("Invalid password for user: {Email}", modelLogin.Email);
                        ViewData["ValidateMessage"] = "Invalid email or password.";
                    }
                }
                else
                {
                    // Log user not found
                    _logger.LogWarning("User not found: {Email}", modelLogin.Email);
                    ViewData["ValidateMessage"] = "Invalid email or password.";
                }
            }

            return View(modelLogin);
        }

        // POST: Access/Logout
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}