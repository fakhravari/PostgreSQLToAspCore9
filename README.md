# Shop API (PostgreSQL + .NET 10 + EF Core 9)

یک Web API تمیز برای فروشگاه با PostgreSQL و EF Core.  
شامل CRUD روی موجودیت‌ها، گزارش‌ها، و نمونه‌های فراخوانی View/Function/Procedure.

---

## 🚀 تکنولوژی‌ها
- **.NET**: `net10.0`
- **EF Core 9** + **Npgsql.EntityFrameworkCore.PostgreSQL**
- **Swagger / Swashbuckle**
- **Newtonsoft.Json** (برای JSON)
- **PostgreSQL 16** (Docker)
- **pgAdmin 4** (Docker)

---

## 📂 ساختار پروژه

```
.
├── Controllers/
│   ├── CategoriesController.cs
│   ├── CustomersController.cs
│   ├── OrdersController.cs
│   ├── ProductsController.cs
│   └── ReportsController.cs
├── Data/
│   └── ShopDbContext.cs
├── Models/
│   ├── Entities.cs
│   └── Dtos.cs
├── appsettings.json
├── Program.cs
└── Properties/launchSettings.json
```

Swagger به صورت پیش‌فرض روی:
```
http://localhost:5196/swagger
```

---

## 🧹 پاکسازی کامل محیط (اختیاری)

```powershell
docker stop pgadmin postgres
docker rm   pgadmin postgres
docker network rm pg-net

Remove-Item -Recurse -Force "C:\Docker\postgres\data"
Remove-Item -Recurse -Force "C:\Docker\pgadmin\data"
```

---

## 🐳 اجرای PostgreSQL و pgAdmin در Docker

```powershell
docker network create pg-net

docker run -d --name postgres --network pg-net --restart unless-stopped `
 -p 5432:5432 `
 -e POSTGRES_USER=appuser `
 -e POSTGRES_PASSWORD=apppass123 `
 -v "C:\Docker\postgres\data:/var/lib/postgresql/data" `
 postgres:16

docker run -d --name pgadmin --network pg-net --restart unless-stopped `
 -p 5050:80 `
 -e PGADMIN_DEFAULT_EMAIL=admin@local.com `
 -e PGADMIN_DEFAULT_PASSWORD=admin123 `
 -v "C:\Docker\pgadmin\data:/var/lib/pgadmin" `
 dpage/pgadmin4:latest
```

pgAdmin UI:  
👉 [http://localhost:5050](http://localhost:5050)  
ورود: `admin@local.com` / `admin123`

---

## 🗄 ساخت دیتابیس و اعمال اسکریپت‌ها

```powershell
docker exec -it postgres psql -U appuser -d postgres -c "DROP DATABASE IF EXISTS shopdb;"
docker exec -it postgres psql -U appuser -d postgres -c "CREATE DATABASE shopdb;"
```

### اعمال اسکریپت‌ها
```powershell
type D:\Repositories\GitHub\PostgreSQLToAspCore9\shopdb_postgres.sql `
| docker exec -i postgres psql -U appuser -d shopdb -v ON_ERROR_STOP=1

type D:\Repositories\GitHub\PostgreSQLToAspCore9\seed_orders_by_month.sql `
| docker exec -i postgres psql -U appuser -d shopdb -v ON_ERROR_STOP=1
```

---

## ⚙️ پیکربندی اتصال در API

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Pg": "Host=localhost;Port=5432;Database=shopdb;Username=appuser;Password=apppass123"
  }
}
```

`Program.cs`:
```csharp
builder.Services.AddDbContext<ShopDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Pg")));
```

---

## ▶️ اجرای API

```powershell
dotnet restore
dotnet run
```

Swagger UI:  
👉 [http://localhost:5196/swagger](http://localhost:5196/swagger)

---

## 📌 Endpoints مهم

### Categories
- `GET /api/categories`
- `POST /api/categories`

### Products
- `GET /api/products`
- `POST /api/products`

### Orders
- `GET /api/orders`
- `POST /api/orders`

### Reports
- `GET /api/reports/products-with-category` (LINQ Join)
- `GET /api/reports/vw_products-with-category` (خواندن ویو)
- `GET /api/reports/orders-by-customer/{id}` (Function)
- `GET /api/reports/products-by-category/{id}` (Procedure + RefCursor)

---

## 📝 نکات EF Core

- در `ShopDbContext` → `NoTracking` پیش‌فرض فعال است.  
- برای Update نیاز به `AsTracking()` یا `Attach/Update` دارید.  
- برای جلوگیری از JSON cycle → از `ReferenceHandler.IgnoreCycles` یا `Newtonsoft.Json` استفاده شده.  

---

## ⚡️ پاکسازی کامل (Reset)

```powershell
docker stop pgadmin postgres
docker rm   pgadmin postgres
docker network rm pg-net

Remove-Item -Recurse -Force "C:\Docker\postgres\data"
Remove-Item -Recurse -Force "C:\Docker\pgadmin\data"
```

---

## 📖 لایسنس
MIT
