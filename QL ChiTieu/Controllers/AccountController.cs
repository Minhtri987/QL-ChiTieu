using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_ChiTieu.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net.Mail;
using System.Net;
namespace QL_ChiTieu.Controllers
{

    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _objModel;



        public AccountController(ApplicationDbContext objModel)
        {
            _objModel = objModel;

        }
        public ActionResult Index()
        {
            if (HttpContext.Session.GetString("idUser") != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }


        // GET : Register
        public ActionResult Register()
        {
            return View();
        }

        //POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User _user)
        {
            if (ModelState.IsValid)
            { return View(_user); }
            try
            {
                if (_objModel.Users.FirstOrDefault(s => s.Email == _user.Email) != null)
                {
                    ViewBag.Error = "Email already exists";
                    return View();
                }
                else
                {
                    _user.Password = GetMD5(_user.Password);
                    _user.ProfilePhotoPath = "/uploads/default.jpg";
                    _user.OTPMail = 0;
                    _objModel.ChangeTracker.AutoDetectChangesEnabled = false;
                    _objModel.Users.Add(_user);
                    _objModel.SaveChanges();
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                // Log or print the exception details
                ViewBag.Error = "An error occurred while saving to the database.";
                return View(_user);
            }

        }

        [AllowAnonymous]
        public IActionResult Login()
        {

            if (HttpContext.Session.GetString("Email") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            HttpContext.Session.Clear();
            TempData["RefreshDashboard"] = true;
            if (ModelState.IsValid)
            {
                var f_password = GetMD5(password);
                var data = _objModel.Users.FirstOrDefault(s => s.Email.Equals(email) && s.Password.Equals(f_password));
                if (data != null)
                {
                    // Lưu thông tin đăng nhập vào session
                    HttpContext.Session.SetString("Email", data.Email.ToString());
                    HttpContext.Session.SetString("idUser", data.idUser.ToString());
                    HttpContext.Session.SetString("FullName", data.FullName().ToString());
                    HttpContext.Session.SetString("ProfilePhotoPath", data.ProfilePhotoPath.ToString());

                    // Kiểm tra thông tin đã được lưu vào session hay chưa
                    var emailFromSession = HttpContext.Session.GetString("Email");
                    var idUserFromSession = HttpContext.Session.GetString("idUser");
                    var fullNameFromSession = HttpContext.Session.GetString("FullName");
                    ViewBag.idUserFromSession = idUserFromSession;
                    if (!string.IsNullOrEmpty(emailFromSession) && !string.IsNullOrEmpty(idUserFromSession) && !string.IsNullOrEmpty(fullNameFromSession))
                    {
                        // Thông tin đã được lưu vào session thành công
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        ViewBag.error = "Failed to save information into session";
                        return View();
                    }
                }
                else
                {
                    ViewBag.error = "Login failed";
                    return View();
                }
            }

            return View();
        }
        //create a string MD5
        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;
        }

        //Logout 
        public ActionResult Logout()
        {

            var transactions = _objModel.Transactions.Where(t => t.idUser != 0).ToList();
            foreach (var transaction in transactions)
            {
                _objModel.Update(transaction);
            }

            _objModel.SaveChanges();

            HttpContext.Session.Clear();
            TempData["RefreshDashboard"] = true;
            HttpContext.Session.Remove("Email");

            return RedirectToAction("Login", "Account");
        }


        //ForgotPassword
        [AllowAnonymous, HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {

            return View();
        }

        [AllowAnonymous, HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _objModel.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    ModelState.Clear();
                    model.EmailSent = true;
                }
                else
                {
                    ModelState.AddModelError("Email", "Email not found in the database");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ProcessEmail()
        {

            return View();
        }
        [HttpPost]

        public IActionResult ProcessEmail(string email)
        {
            // Kiểm tra xem email có hợp lệ không (ở đây kiểm tra đơn giản, bạn có thể thực hiện kiểm tra email phức tạp hơn)
            if (IsValidEmail(email))
            {
                int otp = GenerateRandomNumber();
                var user = _objModel.Users.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    user.OTPMail = otp;
                    _objModel.Users.Update(user);
                    _objModel.SaveChanges();
                    SendNotificationEmail(email, otp);
                    ViewData["Message"] = $"Thông báo đã được gửi đến {email}";
                    ViewData["Email"] = user.Email;
                }
                else
                {
                    ViewData["Message"] = "Địa chỉ Email không tồn tại";
                }
            }
            else
            {
                ViewData["Message"] = "Địa chỉ Email không hợp lệ";
            }

            // Chỉ định tên view là "ProcessEmail"
            return View("ProcessEmail");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.EndsWith("@gmail.com"); // Kiểm tra địa chỉ email và xác nhận là Gmail
            }
            catch
            {
                return false;
            }
        }

        private void SendNotificationEmail(string toEmail, int otp)
        {
            try
            {
                using (var client = new SmtpClient("smtp.gmail.com"))
                {
                    client.Port = 587; // Hoặc 587 nếu bạn muốn sử dụng cổng TLS
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("minhtri5503@gmail.com", "wmap pyvh lssz lyci");
                    client.EnableSsl = true; // Tắt SSL

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("ExpenseTracker@no-reply.com"),
                        Subject = "OTP Reset Password",
                        Body = $"Mã OTP đặt lại mật khẩu là: {otp}",
                        IsBodyHtml = false,
                    };

                    mailMessage.To.Add(toEmail);

                    client.Send(mailMessage);
                }
            }
            catch (SmtpException ex)
            {
                // Xử lý exception
                Console.WriteLine($"SmtpException: {ex.Message}");
            }
        }
        public int GenerateRandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(100000, 1000000);
            return randomNumber;
        }

        [HttpGet]
        public IActionResult OTP(string email)
        {
            var user = _objModel.Users.FirstOrDefault(u => u.Email == email);
            if (string.IsNullOrEmpty(email) || (user != null && user.OTPMail == 0))
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult OTP(int otp, string email)
        {
            var user = _objModel.Users.FirstOrDefault(u => u.Email == email);

            if (user != null && user.OTPMail == otp)
            {
                user.OTPMail = 1;
                _objModel.Users.Update(user);
                _objModel.SaveChanges();
                TempData["PasswordChanged"] = null;
                // Correct OTP, redirect to ChangePassword action
                return RedirectToAction("ChangePassword", new { email });
            }
            else
            {
                // Wrong OTP, display message
                ViewBag.Error = "Wrong OTP";
                ViewBag.Email = email;
                return View();
            }
        }
        [HttpGet]
        public IActionResult ChangePassword(string email)
        {
            var user = _objModel.Users.FirstOrDefault(u => u.Email == email && u.OTPMail == 1);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string newPassword, string confirmNewPassword, string email)
        {
            try
            {
                var user = _objModel.Users.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    if (!string.Equals(newPassword, confirmNewPassword))
                    {
                        ModelState.AddModelError("ConfirmNewPassword", "Mật khẩu mới và xác nhận mật khẩu không trùng khớp.");
                        ViewBag.Email = email;
                        return View();
                    }
                    newPassword = GetMD5(newPassword);
                    user.Password = newPassword;
                    user.OTPMail = 0;
                    _objModel.Users.Update(user);
                    _objModel.SaveChanges();
                    TempData["PasswordChanged"] = true;
                    return RedirectToAction("ChangePassword", new {email });
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                TempData["PasswordChanged"] = false;
                TempData["error"] = ex.Message;
                ViewBag.Email = email;
                return RedirectToAction("ChangePassword", new { email, passwordChanged = false });
            }
        }
    }
}

