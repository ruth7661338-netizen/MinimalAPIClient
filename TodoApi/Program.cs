using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Secrets (connection string, JWT key) are kept in a file that is not committed to git
builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
var app = builder.Build();

app.UseCors();
app.UseAuthentication();   // חדש
app.UseAuthorization();    // חדש

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("", () => "hello");

// חדש: פונקציית עזר שמייצרת טוקן
string CreateToken(User user)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
    };
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(app.Configuration["Jwt:Key"]!));
    var token = new JwtSecurityToken(
        issuer: app.Configuration["Jwt:Issuer"],
        audience: app.Configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
    return new JwtSecurityTokenHandler().WriteToken(token);
}

// חדש: הרשמה
app.MapPost("/register", async (ToDoDbContext db, LoginRequest request) =>
{
    if (await db.Users.AnyAsync(u => u.Username == request.Username))
        return Results.Conflict("Username already exists");

    var user = new User { Username = request.Username, Password = request.Password };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new { token = CreateToken(user) });
});

// חדש: התחברות
app.MapPost("/login", async (ToDoDbContext db, LoginRequest request) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u =>
        u.Username == request.Username && u.Password == request.Password);
    if (user is null) return Results.Unauthorized();

    return Results.Ok(new { token = CreateToken(user) });
});

// בארבעת הנתיבים הקיימים נוסף רק .RequireAuthorization() בסוף
app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync())
    .RequireAuthorization();

app.MapPost("/addTask", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
}).RequireAuthorization();

app.MapPut("/setCompleted/{id}", async (ToDoDbContext db, int id, Item updated) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    if (updated.Name is not null) item.Name = updated.Name;
    item.IsComplete = updated.IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok(item);
}).RequireAuthorization();

app.MapDelete("/deleteTask/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.Run();

// חדש: חייב להיות בסוף הקובץ, אחרי app.Run()
record LoginRequest(string Username, string Password);