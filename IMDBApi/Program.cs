using IMDBApi;
using IMDBApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p =>
    {
        p.AllowAnyHeader().AllowCredentials().AllowAnyMethod().SetIsOriginAllowed(e => true);
    });

});



builder.Services.AddControllers();

builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddDbContext<ImdbDbContext>((options) =>
{
    options.UseNpgsql("Host=imdb.c0ya11pfh1wb.eu-central-1.rds.amazonaws.com;Database=imdb;Username=postgres;Password=tamuro174;");
});

//dotnet ef migrations add Initial --startup-project ./IMDBApi --output-dir ./Data/Migrations

builder.Services.AddSwaggerGen((options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "MES API",
        Description = "MES API Backend Swagger Documentation for API Usage",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Please provide your token like this: 'Bearer {JWT}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {{
                    new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                    new List<string>()
                }});

}));



builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration[key: "Jwt:Key"])
            ),
        };
    });

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IMDB API V1");
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuthMiddleware>();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    using (var aa = scope.ServiceProvider.GetRequiredService<ImdbDbContext>())
    {
        aa.Database.Migrate();
    }

}

app.Run();
