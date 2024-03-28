using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using QL_ChiTieu.Models;
using Microsoft.CodeAnalysis.Scripting;
using System.Security.Cryptography;
using System.Text;
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<ProfileController> logger)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }


    public IActionResult Profile()
    {
        var userIdString = HttpContext.Session.GetString("idUser");

        if (int.TryParse(userIdString, out int userId))
        {
            var user = _context.Users.FirstOrDefault(u => u.idUser == userId);

            if (user != null)
            {
                return View(user);
            }
        }

        return RedirectToAction("Login", "Account");
    }

    public IActionResult EditAvatar()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> EditAvatar(IFormFile avatarFile)
    {
        try
        {
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var idUser = HttpContext.Session.GetString("idUser");
                string uniqueFileName = $"{idUser}_{DateTime.Now:yyyyMMddHHmmss}_profile_photo.jpg";
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Check if the directory exists, create it if not
                if (!Directory.Exists(uploadsFolder))
                {
                    _logger.LogInformation($"Creating directory: {uploadsFolder}");
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Copy the file to the target path
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                    _logger.LogInformation($"File saved successfully to: {filePath}");
                }

                var userIdString = HttpContext.Session.GetString("idUser");

                if (int.TryParse(userIdString, out int userId))
                {
                    var user = _context.Users.FirstOrDefault(u => u.idUser == userId);

                    if (user != null)
                    {
                        user.ProfilePhotoPath = "/uploads/" + uniqueFileName;
                        HttpContext.Session.SetString("ProfilePhotoPath", "/uploads/" + uniqueFileName);
                        _context.SaveChanges();
                    }
                }
            }
            TempData["AvatarUpdated"] = true;
            return RedirectToAction("Profile");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing file upload: {ex.Message}");
            // Handle the exception as needed
            return RedirectToAction("Profile", new { avatarUpdated = false, error = ex.Message });
        }
    }
    public IActionResult EditProfile()
    {
        var userIdString = HttpContext.Session.GetString("idUser");

        if (int.TryParse(userIdString, out int userId))
        {
            var user = _context.Users.FirstOrDefault(u => u.idUser == userId);

            if (user != null)
            {
                return View(user);
            }
        }

        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    public IActionResult EditProfile(User updatedUser, string oldPassword, string newPassword, string confirmNewPassword)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("idUser");

            if (int.TryParse(userIdString, out int userId))
            {
                var user = _context.Users.FirstOrDefault(u => u.idUser == userId);

                if (user != null)
                {
                    oldPassword = GetMD5(oldPassword);
                    // Check if the old password matches the current password
                    if (!string.Equals(oldPassword, user.Password))
                    {
                        ModelState.Remove("NewPassword");
                        ModelState.Remove("ConfirmNewPassword");
                        ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng.");
                        return View(user);
                    }

                    // Update first name and last name
                    user.FirstName = updatedUser.FirstName;
                    user.LastName = updatedUser.LastName;

                    // Check if new password and confirm password match
                    if (!string.Equals(newPassword, confirmNewPassword))
                    {
                        ModelState.Remove("OldPassword");
                        ModelState.AddModelError("ConfirmNewPassword", "Mật khẩu mới và xác nhận mật khẩu không trùng khớp.");
                        return View(user);
                    }

                    // Update password
                    user.Password = GetMD5(newPassword);

                    _context.SaveChanges();

                    TempData["ProfileUpdated"] = true;
                    return RedirectToAction("Profile");
                }
            }

            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating profile: {ex.Message}");
            // Handle the exception as needed
            TempData["ProfileUpdated"] = false;
            return RedirectToAction("Profile", new { profileUpdated = false, error = ex.Message });
        }
    }
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
}
