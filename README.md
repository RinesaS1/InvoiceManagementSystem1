# Invoice Management System

Një sistem i plotë dhe profesional për menaxhimin e faturave (Full-Stack), i ndërtuar me **ASP.NET Core Web API** në backend dhe **React** në frontend, me mbështetje dygjuhëshe (**Shqip / English**), menaxhim të roleve (**Admin** dhe **Customer**), verifikim të pagesave manuale dhe gjenerim të faturave zyrtare në **PDF**.

---

## 🛠️ Teknologjitë e Përdorura (Technology Stack)

### Backend
- **Framework:** .NET 10 (C#) / ASP.NET Core Web API
- **Baza e të Dhënave:** SQLite me Entity Framework Core
- **Autentikimi & Siguria:** ASP.NET Core Identity + JWT Bearer Tokens
- **Gjenerimi i PDF:** QuestPDF
- **Dokumentimi i API-t:** Scalar OpenAPI UI (`/scalar/v1`)
- **Struktura:** Repository/Service pattern me DTOs dhe ApiResponse e unifikuar

### Frontend
- **Biblioteka Kryesore:** React 19 (JavaScript)
- **Vegla e Ndërtimit:** Vite
- **Rrugëtimi (Routing):** React Router DOM v7 (me Protected Routes & Role-Based Access Control)
- **Ndërkombëtarizimi (i18n):** `i18next` + `react-i18next` + `i18next-browser-languagedetector`
- **Klienti HTTP:** Axios (me Request & Response Interceptors për JWT dhe gabimet 401/403)
- **Stilizimi:** CSS3 me Variabla Moderne & Dizajn plotësisht responsive (Mobile/Desktop)

---

## 📁 Struktura e Projektit (Project Structure)

```text
InvoiceManagementSystem1/
├── backend/
│   └── InvoiceManagementSystem.API/
│       ├── Controllers/             # API Endpoints (Auth, Invoices, Payments, Customers)
│       ├── Data/                    # AppDbContext, DbSeeder
│       ├── DTOs/                    # Data Transfer Objects (Auth, Invoices, Payments, Customers)
│       ├── Extensions/              # ServiceExtensions (Database, JWT, Identity, CORS, OpenAPI)
│       ├── Middleware/              # ExceptionMiddleware (Menaxhimi global i gabimeve)
│       ├── Migrations/              # EF Core Migrations
│       ├── Models/                  # Entitetet (ApplicationUser, Invoice, Payment, Enums)
│       ├── Services/                # Logjika e biznesit (Auth, Invoice, Payment, Pdf)
│       ├── appsettings.json         # Konfigurimi bazë (pa sekrete)
│       └── appsettings.Development.json # Konfigurimi lokal (i injoruar nga Git)
│
├── frontend/
│   ├── src/
│   │   ├── admin/                   # Dashboard, Invoices, Customers, Payments, CreateInvoice
│   │   ├── customer/                # Dashboard, MyInvoices, InvoiceDetails
│   │   ├── components/              # Navbar, Layout, Button, FormInput, Alert, Spinner, Badge, ProtectedRoute, LanguageSwitcher
│   │   ├── context/                 # AuthContext (JWT & Sesioni i përdoruesit)
│   │   ├── locales/                 # Fjalorët e përkthimit (sq.json, en.json, i18n.js)
│   │   ├── pages/                   # Login, Register, Unauthorized, NotFound
│   │   ├── services/                # Axios API services (api, auth, invoice, payment, customer)
│   │   ├── App.jsx                  # Konfigurimi i rrugëve të React Router
│   │   ├── index.css                # CSS variablat dhe tipografia
│   │   └── main.jsx
│   ├── .env.example                 # Shembull i variablave të mjedisit
│   └── package.json
│
├── .gitignore
└── README.md
```

---

## ⚙️ Kërkesat Paraprake të Instalimit (Prerequisites)

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v18+ ose v20+) dhe npm](https://nodejs.org/)
- Git

---

## 🗄️ Konfigurimi i Bazës së të Dhënave & Llogarisë së Parë të Administratorit

Baza e të dhënave **SQLite** inicializohet automatikisht në nisjen e parë të backend-it përmes `DbSeeder.cs`.

### Krijimi i `appsettings.Development.json` (Lokalisht):
Në direktorinë `backend/InvoiceManagementSystem.API/`, krijoni skedarin `appsettings.Development.json` (ky skedar injorohet nga Git për të mbrojtur sekretet):

```json
{
  "Jwt": {
    "Key": "Invoice$Mgmt$Dev$Secret$Key$2024!LocalOnly#DoNotUseInProduction",
    "Issuer": "InvoiceManagementSystem",
    "Audience": "InvoiceManagementSystemUsers",
    "ExpiresInMinutes": 120
  },
  "AdminSeed": {
    "Email": "admin@invoiceapp.dev",
    "Password": "Admin@12345",
    "FullName": "Administrator"
  }
}
```

> [!NOTE]
> Gjatë nisjes, `DbSeeder` kontrollon nëse ekzistojnë rolet `Admin` dhe `Customer`. Nëse llogaria e Admin-it nuk ekziston, ajo krijohet automatikisht duke përdorur kredencialet e mësipërme. Fjalëkalimi hashëtohet me ASP.NET Core Identity.

---

## 🚀 Nisja e Aplikacionit (How to Run)

### 1. Nisja e Backend-it (API)
```powershell
cd "backend\InvoiceManagementSystem.API"
dotnet run --launch-profile http
```
- API do të jetë i qasshëm në: `http://localhost:5000`
- Dokumentimi dhe testimi interaktiv (Scalar UI): `http://localhost:5000/scalar/v1`
- Kontrolli i gjendjes: `http://localhost:5000/api/health`

### 2. Nisja e Frontend-it (React)
Në një dritare të re terminali:
```powershell
cd "frontend"
npm install
npm run dev
```
- Frontend do të hapet në: `http://localhost:5173`

---

## 🌟 Funksionalitetet Kryesore (Main Features)

### 1. Siguria & Autentikimi
- **Regjistrim Publik:** Krijon automatikisht vetëm llogari **Customer**. Është e pamundur që nga regjistrimi publik të fitohet roli Admin.
- **JWT Authentication:** Çdo kërkesë private merr automatikisht header-in `Authorization: Bearer <token>`.
- **Backend Authorization:** Serveri mbron çdo burim me `[Authorize(Roles = "Admin")]` ose `[Authorize(Roles = "Customer")]`.

### 2. Paneli i Administratorit (Admin Portal)
- **Dashboard:** Statistika në kohë reale (Totali i faturave, të paguara, të papaguara, shumat monetare, dhe pagesat në pritje).
- **Menaxhimi i Faturave:** Listimi, kërkimi, filtrimi sipas statusit ose klientit, modifikimi i faturës, dhe shkarkimi i PDF.
- **Krijimi i Faturës:** Zgjedhje e klientit ekzistues nga databaza, gjenerim automatik i numrit të faturës, validim i datave dhe shumës.
- **Menaxhimi i Klientëve:** Shikimi i të gjithë klientëve të regjistruar dhe historikut të faturave të tyre.
- **Verifikimi i Pagesave:** Shqyrtimi i njoftimeve manuale të pagesave, konfirmimi (fatura bëhet `Paid`), ose refuzimi me arsye të detyrueshme (fatura mbetet `Unpaid`).

### 3. Paneli i Klientit (Customer Portal)
- **Izolim i Plotë:** Klienti mund të shohë dhe të ndërveprojë **vetëm me faturat e veta**.
- **Dashboard:** Përmbledhje e statusit të pagesave vetjake.
- **Shkarkim PDF:** Shkarkon faturën zyrtare në PDF të gjeneruar nga serveri.
- **Njoftimi i Pagesës ("I Have Paid"):** Klienti dërgon njoftim për faturat e papaguara. Parandalohen njoftimet e dyfishta paralele.
- **Shikimi i Arsyjes së Refuzimit:** Nëse një pagesë refuzohet nga Admini, klienti sheh arsyen dhe mund të dërgojë njoftim të ri.

### 4. Mbështetja Dygjuhëshe (Bilingual Support)
- Ndërrues gjuhe në Navbar (**SQ | EN**).
- 230 çelësa përkthimi me përputhshmëri 100% midis shqipes dhe anglishtes.
- Ruajtje automatike e gjuhës së zgjedhur në `localStorage`.

---

## 🧪 Testimi i Sistemit (System Verification)

Sistemi përmban verifikim të plotë të testuar automatikisht:
- Regjistrimi dhe hyrja e të dy roleve.
- Validimi i bazës së të dhënave (numra unikë fature, shuma pozitive, data e skadencës >= data e lëshimit).
- Izolimi i të dhënave midis klientëve (bllokim me 404/403 i qasjes ndër-klientë).
- Gjenerimi dhe shkarkimi i PDF-ve binarë.
- Pariteti i plotë i përkthimeve dhe suksesi i ndërtimit me `npm run build` (0 gabime).

---

## 📄 Licenca
Ky projekt është zhvilluar si sistem i plotë për menaxhimin e faturave. Të gjitha të drejtat janë të rezervuara.
