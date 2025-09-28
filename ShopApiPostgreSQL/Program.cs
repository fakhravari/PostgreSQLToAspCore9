using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShopApiPostgreSQL.Data;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShopDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Pg")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Shop API (PostgreSQL)", Version = "v1" });
});


builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
    {
        // 1) جلوگیری از حلقه‌های ارجاعی
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        // 2) حذف فیلدهای null
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

        // 3) حفظ PascalCase (غیرفعال‌کردن camelCase)
        options.SerializerSettings.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new DefaultNamingStrategy()
        };
    });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API v1");
        o.RoutePrefix = "swagger";
    });
}

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.Run();
