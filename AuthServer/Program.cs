using AuthServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Authentication START
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
        // Important to set it to zero
        // (by default it is 5 min, so any jwt expire time less than 5 min will still work)
        ClockSkew = TimeSpan.Zero
    };
});
// Injecting SignInManger Dependency.
builder.Services.AddScoped<SignInManager, SignInManager>();
// Authentication END



// Api versioning
builder.Services.AddApiVersioning(options =>
{
    // default api versioning configuration
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
});



// CORS START
builder.Services.AddCors(p => p.AddPolicy("CorsPolicy", policy =>
{
    // NOT SAFE - change the following cors origins with real addresses.
    // CORS requests from any origin with credentials
    policy.SetIsOriginAllowed(origin => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    // build.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost").AllowAnyHeader().AllowCredentials();

}));
// CORS END



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

// Add Authentication for jwt based authentication to work
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
