# دليل التثبيت والنشر — JDMS (MySQL)

## 1. إعداد MySQL على جهازك

1. تأكد أن **MySQL Server** يعمل (الخدمة `MySQL80` أو ما شابه في Services).
2. أنشئ قاعدة البيانات:

```sql
CREATE DATABASE IF NOT EXISTS jdms_db
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
```

أو من سطر الأوامر:

```powershell
mysql -u root -p -e "CREATE DATABASE jdms_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
```

3. (اختياري) مستخدم مخصص:

```sql
CREATE USER 'jdms_user'@'localhost' IDENTIFIED BY 'YourStrongP@ssw0rd!';
GRANT ALL PRIVILEGES ON jdms_db.* TO 'jdms_user'@'localhost';
FLUSH PRIVILEGES;
```

## 2. إعداد سلسلة الاتصال

عدّل `src/JDMS.Web/appsettings.Development.json` (للتطوير المحلي):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=jdms_db;User=root;Password=YOUR_ROOT_PASSWORD;"
  }
}
```

| الحقل | الوصف |
|--------|--------|
| Server | عادة `localhost` |
| Port | عادة `3306` |
| Database | `jdms_db` |
| User | `root` أو `jdms_user` |
| Password | كلمة مرور MySQL لديك |

## 3. تطبيق مخطط قاعدة البيانات

**الطريقة أ — تلقائياً عند تشغيل التطبيق (موصى بها):**

```powershell
cd e:\Gra-project
dotnet run --project src/JDMS.Web
```

يتم تطبيق الهجرات وزرع البيانات عند أول تشغيل.

**الطريقة ب — EF يدوياً:**

```powershell
dotnet ef database update --project src/JDMS.Infrastructure --startup-project src/JDMS.Web
```

**الطريقة ج — سكربت SQL:**

نفّذ `database/JDMS_Database_MySQL.sql` في MySQL Workbench على قاعدة `jdms_db`.

## 4. التحقق

1. افتح المتصفح: `https://localhost:7xxx` (راجع `launchSettings.json` للمنفذ).
2. سجّل الدخول: `admin` / `Admin@123`
3. غيّر كلمة مرور المدير من **المستخدمين**.

## 5. النشر على IIS

1. ثبّت [.NET 8 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0).
2. تأكد أن خادم IIS يصل إلى MySQL (نفس الجهاز أو `Server=IP` في Connection String).
3. ضع Connection String في `appsettings.Production.json` أو متغير البيئة:
   `ConnectionStrings__DefaultConnection`
4. انشر المشروع:
   ```powershell
   dotnet publish src/JDMS.Web/JDMS.Web.csproj -c Release -o C:\inetpub\wwwroot\jdms
   ```

## 6. استكشاف الأخطاء

| المشكلة | الحل |
|---------|------|
| Access denied for user | تحقق من User/Password في appsettings |
| Can't connect to server | شغّل خدمة MySQL، تحقق من Port 3306 |
| Unknown database | أنشئ `jdms_db` أولاً |
| Authentication plugin | استخدم MySQL 8 مع `mysql_native_password` أو مستخدم حديث |

### اختبار الاتصال من PowerShell

```powershell
mysql -u root -p -e "SHOW DATABASES LIKE 'jdms_db';"
```

## 7. ملفات قاعدة البيانات

| الملف | الاستخدام |
|-------|-----------|
| `database/JDMS_MySQL_Setup.sql` | إنشاء قاعدة البيانات فقط |
| `database/JDMS_Database_MySQL.sql` | الجداول الكاملة (من EF) |
| `database/JDMS_Database.sql` | **قديم — SQL Server فقط** |
