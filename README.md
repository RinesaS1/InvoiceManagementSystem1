# 🧾 Invoice Management System — Sistemi i Menaxhimit të Faturave

> A full-stack web application for managing invoices, built as a recruitment assignment.
> Aplikacion web full-stack për menaxhimin e faturave, ndërtuar si detyrë rekrutimi.

---

## 🌐 Technologies / Teknologjitë

| Layer              | Technology                        |
|--------------------|-----------------------------------|
| Frontend           | React (JavaScript)                |
| Backend            | C# — ASP.NET Core Web API         |
| Database           | SQLite + Entity Framework Core    |
| Authentication     | ASP.NET Core Identity + JWT       |
| PDF Generation     | .NET PDF Library (TBD)            |
| Languages / Gjuhët | 🇦🇱 Albanian / 🇬🇧 English         |

---

## 📁 Project Structure / Struktura e Projektit

```
InvoiceManagementSystem1/
├── frontend/          # React application / Aplikacioni React
├── backend/           # ASP.NET Core Web API
├── .gitignore         # Git ignore rules
└── README.md          # This file / Ky skedar
```

---

## 🚀 Getting Started / Fillimi

### Prerequisites / Parakushtet
- Node.js (v18+)
- .NET SDK (v8+)
- SQLite

### Frontend
```bash
cd frontend
npm install
npm start
```

### Backend
```bash
cd backend
dotnet restore
dotnet run
```

---

## 🔐 Authentication / Autentifikimi

This application uses **JWT (JSON Web Tokens)** together with **ASP.NET Core Identity** for secure authentication and role-based authorization.

Ky aplikacion përdor **JWT** së bashku me **ASP.NET Core Identity** për autentifikim të sigurt dhe autorizim me role.

---

## 📄 License / Licenca

This project is for recruitment/educational purposes only.
Ky projekt është vetëm për qëllime rekrutimi/edukimi.
