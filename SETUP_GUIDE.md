# دليل تشغيل مشروع Agridome على جهاز جديد (خطوة بخطوة)

هذا الدليل لشخص **مش مبرمج**. اتبع الخطوات بالترتيب حرفياً.

النظام مكوّن من 3 أجزاء:
1. **الباك إند (Backend)** — السيرفر، مكتوب بـ ASP.NET Core (لغة C#).
2. **قاعدة البيانات (Database)** — SQL Server LocalDB.
3. **الفرونت إند (Frontend)** — الموقع، مكتوب بـ React.

سنشغّل الثلاثة على نفس اللابتوب.

---

## الخطوة 1: تنصيب البرامج المطلوبة

نزّل وثبّت هذه البرامج بالترتيب (كلها مجانية):

| # | البرنامج | الرابط | ملاحظة عند التثبيت |
|---|---|---|---|
| 1 | **Git** | https://git-scm.com/download/win | اضغط Next على كل الخطوات (الافتراضي تمام) |
| 2 | **Visual Studio 2022 Community** | https://visualstudio.microsoft.com/downloads/ | ⚠️ مهم جداً: في شاشة Workloads، علّم على **"ASP.NET and web development"**. وتأكد إن **"SQL Server Express LocalDB"** مُحدّد (يكون ضمنها عادةً) |
| 3 | **.NET 9 SDK** | https://dotnet.microsoft.com/download/dotnet/9.0 | نزّل "SDK x64" وثبّته |
| 4 | **Node.js (LTS)** | https://nodejs.org | نزّل النسخة LTS، Next على كل شي |

> بعد تثبيت Visual Studio، لو طلب منك تسجيل دخول بحساب Microsoft، سجّل بحساب عادي (مجاني).

---

## الخطوة 2: تحميل الكود من GitHub

1. افتح برنامج **Git Bash** (انضاف مع Git) أو **Command Prompt**.
2. اكتب الأمر التالي (يحمّل المشروع لمجلد على سطح المكتب مثلاً):
   ```
   cd Desktop
   git clone https://github.com/diaa-mo7mad/test.git Agridome
   ```
3. صار عندك مجلد اسمه **Agridome** فيه كل الكود.

> بديل بدون Git: افتح رابط الـ repo بالمتصفح → زر **Code** الأخضر → **Download ZIP** → فك الضغط.

---

## الخطوة 3: ملف الأسرار (appsettings.json)

ملف الإعدادات السري **غير موجود في GitHub** (لأسباب أمنية). لازم تضيفه يدوياً:

**الطريقة الأسهل:** اطلب من صاحب المشروع يرسللك ملف **`appsettings.json`** الجاهز، وضعه داخل المجلد:
```
Agridome/ARGI.PL/appsettings.json
```

**الطريقة البديلة (لو ما عندك الملف):**
1. ادخل مجلد `Agridome/ARGI.PL/`
2. انسخ الملف `appsettings.example.json` وسمّ النسخة `appsettings.json`
3. افتحه بـ Notepad وعبّي القيم:
   - `Jwt:SecretKey` → أي نص طويل عشوائي (مثلاً 30 حرف وأرقام)
   - `Anthropic:ApiKey` → مفتاح Claude API (يطلبه صاحب المشروع، أو من console.anthropic.com — مدفوع)

> 💡 لو ما حطّيت مفتاح Anthropic، النظام يشتغل عادي بس توصيات الذكاء الاصطناعي تستخدم قيم احتياطية بدل توليد نصوص ذكية. السقاية والمراقبة تشتغل طبيعي.

---

## الخطوة 4: إنشاء قاعدة البيانات

1. افتح ملف **`Agridome/ARGI.sln`** بالدبل-كليك (يفتح بـ Visual Studio).
2. انتظر لين Visual Studio يحمّل المشروع ويسترجع المكتبات (Restore) — أول مرة تاخذ دقيقتين.
3. من القائمة العلوية: **Tools → NuGet Package Manager → Package Manager Console**.
4. فوق نافذة الـ Console، في خانة **"Default project"** اختر **ARGI.DAL**.
5. اكتب هذا الأمر واضغط Enter:
   ```
   Update-Database
   ```
6. انتظر لين يطلع **"Done."** — هذا أنشأ قاعدة البيانات وكل الجداول تلقائياً.

> المكتبات (NuGet) لكل الباك إند تتحمّل **تلقائياً** عند فتح المشروع — ما تحتاج تثبّت شي يدوياً.

---

## الخطوة 5: تشغيل الباك إند

1. في Visual Studio، فوق، تأكد إن المشروع المختار للتشغيل هو **ARGI.PL** (يكون بالخط العريض في Solution Explorer؛ لو لأ: كليك يمين على ARGI.PL → **Set as Startup Project**).
2. جنب زر التشغيل الأخضر، اختر الملف الشخصي **http** (مش https — أسهل).
3. اضغط **زر التشغيل الأخضر** (أو F5).
4. تفتح نافذة سوداء (Console) وتظل شغّالة — **هذا معناه الباك إند يعمل** على العنوان:
   ```
   http://localhost:5163
   ```
   **اترك هذه النافذة مفتوحة.**

> للتأكد: افتح المتصفح على `http://localhost:5163/scalar/v1` — لازم تشوف صفحة توثيق الـ API.

---

## الخطوة 6: تشغيل الفرونت إند وربطه بالباك إند

### أ. اضبط رابط الباك إند في الفرونت
1. افتح الملف:
   ```
   Agridome/frontend/src/services/api.js
   ```
   (بأي محرر نصوص، حتى Notepad)
2. في أول الملف، غيّر السطر:
   ```js
   export const BASE_URL = 'https://3nfmpd2b-5163.euw.devtunnels.ms';
   ```
   إلى عنوان الباك إند المحلي:
   ```js
   export const BASE_URL = 'http://localhost:5163';
   ```
3. احفظ الملف.

### ب. شغّل الفرونت إند
1. افتح **Command Prompt** أو **Git Bash**.
2. ادخل مجلد الفرونت:
   ```
   cd Desktop/Agridome/frontend
   ```
3. ثبّت مكتبات الفرونت (أول مرة فقط، تاخذ دقيقة-دقيقتين):
   ```
   npm install
   ```
4. شغّل الموقع:
   ```
   npm run dev
   ```
5. يطلع رابط:
   ```
   http://localhost:5173
   ```
   افتحه بالمتصفح — **هذا هو الموقع**.

> **مهم:** الباك إند (نافذة Visual Studio) لازم يضل شغّال طول ما تستخدم الموقع.

---

## كيف يتربط كل شي مع بعض (ملخّص)

```
الفرونت إند (المتصفح localhost:5173)
        ↓  يرسل طلبات إلى BASE_URL
الباك إند (localhost:5163)
        ↓  يتصل بـ
قاعدة البيانات (LocalDB)
```

- **الفرونت ↔ الباك:** عن طريق `BASE_URL` في ملف `api.js` (الخطوة 6-أ).
- **الباك ↔ قاعدة البيانات:** عن طريق `ConnectionStrings:DefaultConnection` في `appsettings.json` (محطوط تلقائياً على LocalDB).
- الباك إند يسمح بالاتصال من `localhost:5173` (CORS مضبوط مسبقاً) — ما تحتاج تعدّل شي.

---

## أول استخدام

1. افتح `http://localhost:5173`.
2. اضغط **Register** وسجّل حساب جديد (الإيميل يتأكّد تلقائياً، تدخل مباشرة).
   - ⚠️ كلمة المرور لازم: حرف كبير + حرف صغير + رقم + رمز (مثل `@`) + 6 خانات على الأقل. مثال: `Admin@123`
3. بتدخل على صفحة المزارع → عدّل المزرعة وحط بياناتها.

---

## (اختياري) ربط قطعة ESP32

إذا بدك تربط ESP32 حقيقي:
1. الـ ESP32 جهاز منفصل، فما يقدر يستخدم `localhost`. يحتاج **IP اللابتوب على الشبكة** (مثلاً `192.168.1.50`).
2. في كود ESP32، غيّر `serverBaseUrl` إلى `http://<IP_اللابتوب>:5163`.
3. لازم تخلّي الباك إند يستمع على الشبكة (مش localhost فقط): في `ARGI.PL/Properties/launchSettings.json` غيّر `http://localhost:5163` إلى `http://0.0.0.0:5163`، وتسمح للبرنامج في جدار الحماية (Firewall).
4. تأكد إن الـ MAC في كود ESP32 مطابق للـ MAC المسجّل في المزرعة.

> هذه خطوة متقدمة — لو بس بدك تشغّل الموقع للعرض، تجاهلها.

---

## حل المشاكل الشائعة

| المشكلة | الحل |
|---|---|
| الموقع يقول "Login failed" أو الطلبات تفشل | تأكد إن الباك إند شغّال (نافذة VS مفتوحة)، وإن `BASE_URL` = `http://localhost:5163` |
| خطأ "Invalid column name ..." عند التشغيل | ما طبّقت `Update-Database` (الخطوة 4) |
| `npm` أمر غير معروف | ما ثبّت Node.js، أو لازم تسكّر وتفتح الـ Command Prompt من جديد |
| خطأ LocalDB / قاعدة البيانات ما تتصل | افتح المشروع من **Visual Studio** وشغّله من هناك (LocalDB يشتغل مع VS) |
| كلمة المرور مرفوضة عند التسجيل | لازم حرف كبير وصغير ورقم ورمز و6 خانات (مثل `Admin@123`) |
| "Please confirm your email" | استخدم حساب سجّلته بنفسك (التسجيل يؤكّد تلقائياً) |

---

## باختصار — قائمة سريعة

1. ثبّت: Git + Visual Studio 2022 (مع ASP.NET workload) + .NET 9 SDK + Node.js
2. `git clone` المشروع
3. حط `appsettings.json` في `ARGI.PL/`
4. افتح `ARGI.sln` → Package Manager Console → `Update-Database`
5. شغّل الباك إند (F5) → `http://localhost:5163`
6. عدّل `BASE_URL` في `frontend/src/services/api.js` إلى `http://localhost:5163`
7. في مجلد `frontend`: `npm install` ثم `npm run dev` → `http://localhost:5173`
8. سجّل حساب وابدأ
