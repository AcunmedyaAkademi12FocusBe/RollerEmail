using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Roller.Data;
using Roller.Models;
using Roller.Models.Emails;

namespace Roller.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController(IRecaptchaValidator recaptchaValidator, IFluentEmail fluentEmail, AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) : ControllerBase
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return Ok(new { msg = "Merhaba" });
    }
    
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return Ok(new { msg = "Merhaba Admin" });
    }

    [HttpGet("make-admin")]
    public async Task<IActionResult> MakeAdmin()
    {
        var userId = userManager.GetUserId(User);
        var user = await userManager.FindByIdAsync(userId);

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        
        // IdentityResult döndüğü için sonucu değişkene alıp
        // içindeki Succeded prop'unu kontrol ederek geri bildirim verebiliriz.
        await userManager.AddToRoleAsync(user, "Admin");
        
        // güvenlik stamp'ini elle update ettiğimiz için
        // eğer kullanıcı login ise oturumu düşer
        // kullanıcıya rol ataması yaptığımızda veya rolünü kaldırdığımızda
        // bunu yaparsak yeniden login olmak zorunda kalır.
        await userManager.UpdateSecurityStampAsync(user);
        
        return Ok(new { msg = "Admin oldun hadi iyisin!" });
    }

    [HttpGet("email")]
    public IActionResult SendEmail()
    {
        var userEmail = "orhanekici@gmail.com";

        // var template = "Merhaba @Model.Name;<br>Bu template üzerinden gönderdiğim e-posta!";

        var model = new WelcomeEmail
        {
            Name = "Orhan",
            Message = "Naber canım?"
        };
        model.Features.AddRange(
            "Deneme 1", "Deneme 1 2"
        );
        
        var email = fluentEmail
            .To(userEmail)
            .Subject("Uygulama üzerinden e-posta merhaba")
            .UsingTemplateFromFile("Emails/Welcome.cshtml", model)
            // TODO: email templatelerinin ana klasörü için ayar yapmamız lazım.
            // her seferinde tüm path'i yazmayalım.
            // .UsingTemplateFromFile("Views/Emails/Welcome.cshtml", model)
            // .UsingTemplate(template, new { Name = "Orhan" })
            //.Body("merhaba bu <strong>ikinci</strong> e-postam. lütfen çalış", true)
            .Send();
        
        return NoContent();
    }

    [HttpPost("/deneme")]
    [AllowAnonymous]
    public async Task<IActionResult> Deneme(Deneme model)
    {
        // static method -> en alıştığımız kolay çözüm - eğer başka bir şey yapamıyorsak mükemmel - test edilebilirliği düşük-yok
        // servis - DI -> alternatif en iyi yaklaşımlardan biri. kendi iş akışımız içinde istediğimiz sırada yapmamıza olanak sağlar
        // data annotation -> dışa bağımlı olması işi bozuyor yoksa harika, statik test yazmak çok zor
        // action filter -> bu mantıklı olur
        // middleware -> dışa bağımlı dolayısıyla performansı kontrol edemiyorum

        if (!await recaptchaValidator.IsValidAsync(model.RecaptchaResponse))
        {
            return Ok(new { Msg = "yoo dostum captchasız olmaz..." });
        }

        // var isCaptchaValid = await recaptchaValidator.IsValidAsync(model.RecaptchaResponse);
        return Ok(new { Msg = "Hi"});
    }
}