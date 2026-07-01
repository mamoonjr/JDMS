# نشر JDMS على Render.com + Aiven MySQL

## قبل البدء (يجب أن يكون جاهزاً)

| # | المطلوب | الحالة |
|---|---------|--------|
| 1 | المشروع على **GitHub** (`mamoonjr/JDMS`) | ✓ |
| 2 | قاعدة **MySQL على Aiven** تعمل | من لوحة Aiven |
| 3 | نسخ **Host, Port, Database, User, Password** من Aiven | بدون مسافات زائدة |
| 4 | في Aiven → **Allowed inbound connections** | أضف `0.0.0.0/0` مؤقتاً أو IP ثابت من Render |

---

## خطوات إنشاء الخدمة على Render

### 1. New → Web Service

- اربط مستودع GitHub: `mamoonjr/JDMS`
- الفرع: `main`

### 2. الإعدادات الأساسية

| الحقل | القيمة |
|--------|--------|
| **Name** | `jdms` (أو أي اسم) |
| **Region** | الأقرب لك (مثلاً Frankfurt) |
| **Branch** | `main` |
| **Runtime** | **Docker** |
| **Dockerfile Path** | `Dockerfile` |
| **Instance Type** | Free (للتجربة) أو Paid (للإنتاج) |

> Render يكتشف `Dockerfile` في جذر المشروع تلقائياً.

### 3. Environment Variables (مهم جداً)

اضغط **Advanced → Add Environment Variable**:

| Key | Value | ملاحظة |
|-----|--------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | إلزامي |
| **طريقة 1 (موصى بها)** | متغيرات منفصلة أدناه | تتجنب مشاكل كلمة المرور |
| **طريقة 2** | `ConnectionStrings__DefaultConnection` | سلسلة واحدة — انظر الأسفل |
| `PORT` | يضبطه Render تلقائياً | لا تغيّره عادة |

### الطريقة 1 — متغيرات منفصلة (الأفضل على Render)

| Key | مثال من Aiven |
|-----|----------------|
| `MYSQL_HOST` | `mysql-xxxxx.a.aivencloud.com` |
| `MYSQL_PORT` | `12345` |
| `MYSQL_DATABASE` | `defaultdb` |
| `MYSQL_USER` | `avnadmin` |
| `MYSQL_PASSWORD` | كلمة المرور من Aiven |

### الطريقة 2 — سلسلة اتصال واحدة

**مهم:** يجب أن تحتوي على `Database=defaultdb` — بدونها يظهر الخطأ `database ''`.

```
Server=mysql-xxxxx.a.aivencloud.com;Port=12345;Database=defaultdb;User=avnadmin;Password=كلمة_المرور;SslMode=Required;
```

انسخ القيم من Aiven → **Connection information** (Host, Port, Database name, User, Password).

### 4. Health Check (اختياري موصى به)

| الحقل | القيمة |
|--------|--------|
| **Health Check Path** | `/Account/Login` |

### 5. Create Web Service

- أول نشر يستغرق 5–15 دقيقة (بناء Docker + هجرات EF تلقائياً).
- عند النجاح يظهر رابط: `https://jdms-xxxx.onrender.com`

---

## بعد النشر

1. افتح الرابط العام من Render.
2. سجّل الدخول: `admin` / `Admin@123`
3. **غيّر كلمة مرور المدير فوراً** من المستخدمين والصلاحيات.
4. ارفع شعار الشركة من إعدادات الشركة (الملفات تُحفظ على قرص مؤقت في Free tier).

---

## ملاحظات Render Free

| الموضوع | التفاصيل |
|---------|----------|
| **النوم** | الخدمة تتوقف بعد عدم استخدام → أول فتح بطيء (~30 ثانية) |
| **القرص** | رفع الصور يُفقد عند إعادة النشر (استخدم Paid + Disk أو تخزين خارجي لاحقاً) |
| **HTTPS** | Render يوفره تلقائياً على `*.onrender.com` |
| **الدومين الخاص** | Settings → Custom Domains → أضف دومينك |

---

## استكشاف الأخطاء

| الخطأ | الحل |
|-------|------|
| Build failed | راجع **Logs** في Render؛ تأكد من وجود `Dockerfile` في الجذر |
| Access denied (MySQL) | تحقق من User/Password في متغير البيئة |
| Can't connect to server | فعّل `0.0.0.0/0` في Aiven أو أضف IP Render |
| `database ''` / Database فارغ | تأكد من `Database=defaultdb` في السلسلة — لا تنسخ من Aiven بدون اسم القاعدة |
| Transient failure / EnableRetryOnFailure | أصلح السلسلة أولاً؛ التطبيق يعيد المحاولة تلقائياً الآن |
| لصق رابط `mysql://` من Aiven | مدعوم تلقائياً، أو استخدم صيغة `Server=...;Database=...` |
| صفحة بيضاء / 502 | تأكد أن التطبيق يستمع على `PORT` (مدعوم في `Program.cs`) |

---

## أوامر مفيدة محلياً (اختبار Docker قبل Render)

```powershell
cd e:\Gra-project
docker build -t jdms .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="Server=...;SslMode=Required;" jdms
```

ثم افتح: http://localhost:8080
