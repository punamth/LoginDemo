using LoginApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginApp.Controllers
{
    public class AccountController : Controller
    {
        private const string SessionUserKey = "Users";
        private const string SessionTokenKey = "JwtToken";

        // ========== REGISTER ========== //
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(AuthViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve existing users from session (or empty list if none)
                var users = HttpContext.Session.GetObject<List<User>>(SessionUserKey) ?? new List<User>();

                // Add new user
                users.Add(new User { Username = model.Username, Password = model.Password });

                // Save updated user list back to session
                HttpContext.Session.SetObject(SessionUserKey, users);

                TempData["Success"] = "Registered successfully!";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // ========== LOGIN ========== //
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(AuthViewModel model)
        {
            var users = HttpContext.Session.GetObject<List<User>>(SessionUserKey) ?? new List<User>();
            var user = users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                // Generate JWT token
                var token = GenerateJwtToken(user.Username);

                // Store token in session for future use (e.g., API calls)
                HttpContext.Session.SetString(SessionTokenKey, token);

                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid credentials";
            return View(model);
        }

        // ========== DASHBOARD ========== //
        public IActionResult Dashboard()
        {
            ViewBag.Token = HttpContext.Session.GetString(SessionTokenKey);
            return View();
        }

        // ========== JWT TOKEN GENERATION ========== //
        private string GenerateJwtToken(string username)
        {
            var secret = "2bT!e5J9xZrQwMnLsD3HvKcYp8Ef@123"; // ✅ 32+ characters
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                issuer: "yourApp",
                audience: "yourAppUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
