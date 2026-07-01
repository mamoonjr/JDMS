# Jordan Delivery Management System (JDMS)

نظام إدارة الطلبات وتتبع التوصيل للمؤسسات — مبني على ASP.NET Core 8 MVC مع Clean Architecture.

## المتطلبات

- Windows Server 2019+ أو Windows 10/11
- Visual Studio 2022 (17.8+)
- .NET 8 SDK
- MySQL 8.0+ (مثبت على جهازك)
- IIS 10+ (للنشر)

## هيكل الحل

```
JDMS.sln
├── src/JDMS.Domain          — الكيانات والتعدادات
├── src/JDMS.Application     — الواجهات، DTOs، الثوابت
├── src/JDMS.Infrastructure  — EF Core، المستودعات، الخدمات، الهوية
└── src/JDMS.Web             — MVC، الواجهات العربية RTL
```

## التشغيل السريع (التطوير)

1. إنشاء قاعدة البيانات في MySQL (أو نفّذ `database/JDMS_MySQL_Setup.sql`):
   ```sql
   CREATE DATABASE jdms_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```

2. عدّل كلمة مرور MySQL في `src/JDMS.Web/appsettings.Development.json` (أو `appsettings.json`):
   ```json
   "DefaultConnection": "Server=localhost;Port=3306;Database=jdms_db;User=root;Password=YOUR_PASSWORD;"
   ```

3. من مجلد الحل:
   ```powershell
   cd e:\Gra-project
   dotnet restore
   dotnet build
   dotnet run --project src/JDMS.Web
   ```

4. عند أول تشغيل يتم تطبيق الهجرات وزرع البيانات تلقائياً.

5. تسجيل الدخول:
   - **المستخدم:** `admin`
   - **كلمة المرور:** `Admin@123`

## الأدوار

| الدور | الصلاحيات |
|--------|-----------|
| Administrator | كامل الصلاحيات + إدارة المستخدمين |
| Manager | التقارير، السائقين، المحافظات، سجل التدقيق |
| Employee | العملاء، الطلبات، التوصيل، الفواتير |

## الميزات الرئيسية

- لوحة تحكم بإحصائيات ورسوم Chart.js
- إدارة العملاء، العناوين، المنتجات، الطلبات
- 12 محافظة أردنية مع مناطق ورسوم توصيل
- تعيين السائقين وتتبع التوصيل
- فواتير PDF (QuestPDF)
- تقارير مع تصدير Excel و PDF
- سجل تدقيق للأنشطة والتغييرات
- واجهة عربية RTL مع دعم الوضع الداكن

## النشر على IIS

راجع [docs/INSTALLATION.md](docs/INSTALLATION.md) للتعليمات الكاملة.

## قاعدة البيانات

- سكربت MySQL كامل: `database/JDMS_Database_MySQL.sql`
- إنشاء قاعدة البيانات فقط: `database/JDMS_MySQL_Setup.sql`
- هجرات EF: `src/JDMS.Infrastructure/Data/Migrations/`

## التقنيات

- ASP.NET Core 8 MVC + Cookie Authentication
- Entity Framework Core 8 + MySQL (Pomelo)
- Bootstrap 5 RTL, jQuery, DataTables, Chart.js
- QuestPDF, ClosedXML
- Repository + Unit of Work

## الترخيص

مشروع تعليمي/مؤسسي — يمكن تخصيصه حسب احتياجاتكم.
