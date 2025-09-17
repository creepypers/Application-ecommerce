using Microsoft.AspNetCore.Mvc;
using TPECOM.Models;
using TPECOM.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TPECOM.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        
        public AccountController(AppDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            
            if (user == null)
            {
                ModelState.AddModelError("", "Email ou mot de passe incorrect.");
                return View();
            }
            
            // Stocker les informations de l'utilisateur dans la session
            HttpContext.Session.SetString("User_Email", user.Email);
            HttpContext.Session.SetString("User_FirstName", user.FirstName);
            HttpContext.Session.SetString("User_LastName", user.LastName);
            HttpContext.Session.SetString("User_Type", user.UserType.ToString());
            HttpContext.Session.SetInt32("User_Id", user.Id);
            
            // Rediriger vers la page d'accueil ou le tableau de bord
            if (user.UserType == UserType.Vendeur)
            {
                return RedirectToAction("Dashboard", "Seller");
            }
            
            return RedirectToAction("Index", "Home");
        }
        
        public IActionResult Register()
        {
            return View(new User());
        }
        
        [HttpPost]
        public IActionResult Register(User user, string ConfirmPassword)
        {
            // Vérifier si l'email est déjà utilisé
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                return View(user);
            }
            
            // Vérifier si les mots de passe correspondent
            if (user.Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Les mots de passe ne correspondent pas.");
                return View(user);
            }
            
            // Ajouter l'utilisateur à la base de données
            _context.Users.Add(user);
            _context.SaveChanges();
            
            // Stocker les informations de l'utilisateur dans la session
            HttpContext.Session.SetString("User_Email", user.Email);
            HttpContext.Session.SetString("User_FirstName", user.FirstName);
            HttpContext.Session.SetString("User_LastName", user.LastName);
            HttpContext.Session.SetString("User_Type", user.UserType.ToString());
            HttpContext.Session.SetInt32("User_Id", user.Id);
            
            // Rediriger vers la page d'accueil ou le tableau de bord
            if (user.UserType == UserType.Vendeur)
            {
                TempData["SuccessMessage"] = "Votre compte vendeur a été créé avec succès !";
                return RedirectToAction("Dashboard", "Seller");
            }
            
            TempData["SuccessMessage"] = "Votre compte a été créé avec succès !";
            return RedirectToAction("Index", "Home");
        }
        
        public IActionResult Profile()
        {
            // Récupérer les informations de l'utilisateur depuis la session
            var email = HttpContext.Session.GetString("User_Email");
            
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }
            
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            
            return View(user);
        }
        
        [HttpPost]
        public IActionResult UpdateProfile(User updatedUser)
        {
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            
            // Vérifier si l'email a été modifié et s'il est déjà utilisé par un autre utilisateur
            if (user.Email != updatedUser.Email && _context.Users.Any(u => u.Email == updatedUser.Email && u.Id != userId))
            {
                TempData["ErrorMessage"] = "Cet email est déjà utilisé par un autre utilisateur.";
                return RedirectToAction("Profile");
            }
            
            // Mettre à jour les informations de l'utilisateur
            user.Email = updatedUser.Email;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            
            if (user.UserType == UserType.Vendeur)
            {
                user.CompanyName = updatedUser.CompanyName;
                user.CompanyAddress = updatedUser.CompanyAddress;
                user.CompanyPhone = updatedUser.CompanyPhone;
                user.CompanyDescription = updatedUser.CompanyDescription;
            }
            
            _context.Users.Update(user);
            _context.SaveChanges();
            
            // Mettre à jour la session
            HttpContext.Session.SetString("User_Email", user.Email);
            HttpContext.Session.SetString("User_FirstName", user.FirstName);
            HttpContext.Session.SetString("User_LastName", user.LastName);
            
            TempData["SuccessMessage"] = "Votre profil a été mis à jour avec succès !";
            return RedirectToAction("Profile");
        }
        
        [HttpPost]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmNewPassword)
        {
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            
            // Vérifier si le mot de passe actuel est correct
            if (user.Password != CurrentPassword)
            {
                TempData["ErrorMessage"] = "Le mot de passe actuel est incorrect.";
                return RedirectToAction("Profile");
            }
            
            // Vérifier si les nouveaux mots de passe correspondent
            if (NewPassword != ConfirmNewPassword)
            {
                TempData["ErrorMessage"] = "Les nouveaux mots de passe ne correspondent pas.";
                return RedirectToAction("Profile");
            }
            
            // Vérifier si le nouveau mot de passe a au moins 6 caractères
            if (NewPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Le nouveau mot de passe doit contenir au moins 6 caractères.";
                return RedirectToAction("Profile");
            }
            
            // Mettre à jour le mot de passe
            user.Password = NewPassword;
            _context.Users.Update(user);
            _context.SaveChanges();
            
            TempData["SuccessMessage"] = "Votre mot de passe a été modifié avec succès !";
            return RedirectToAction("Profile");
        }
        
        public IActionResult Logout()
        {
            // Supprimer les informations de l'utilisateur de la session
            HttpContext.Session.Remove("User_Email");
            HttpContext.Session.Remove("User_FirstName");
            HttpContext.Session.Remove("User_LastName");
            HttpContext.Session.Remove("User_Type");
            HttpContext.Session.Remove("User_Id");
            
            return RedirectToAction("Index", "Home");
        }

        public User GetUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }
    }
} 