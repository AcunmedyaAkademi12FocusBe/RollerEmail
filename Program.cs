using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Roller;
using Roller.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<IdentityUser>(opt =>
    {
        // yayında bunları yapmayın lütfen :)
        opt.Password.RequiredLength = 1;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequiredUniqueChars = 0;
        opt.Password.RequireDigit = false;
        opt.Password.RequiredUniqueChars = 0;
        opt.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// kullanıcının rolünü değiştirdiğimizde oturumunu düşürebilmek için bu intervali 0 olarak ayarlıyoruz
builder.Services.Configure<SecurityStampValidatorOptions>(opt => opt.ValidationInterval = TimeSpan.Zero);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// POCO class -> Plain Old CLR Object
var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
builder.Services
    .AddFluentEmail(smtpSettings.FromEmail, smtpSettings.FromName)
    .AddRazorRenderer(Path.Combine(builder.Environment.ContentRootPath, "Views", "Emails"))
    .AddSmtpSender(new SmtpClient(smtpSettings.Host, smtpSettings.Port)
    {
        EnableSsl = true,
        Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGroup("Auth").MapIdentityApi<IdentityUser>().WithTags("Auth");

app.Run();