# Invoice Management System

A full-stack Invoice Management System built with **ASP.NET Core Web API** and **React**. The application provides role-based portal experiences for Administrators and Customers, server-side PDF invoice generation, a manual payment verification workflow, and full bilingual support (English and Albanian).

This project was developed as a technical recruitment assignment to demonstrate clean software architecture, secure authentication, and end-to-end full-stack integration.

---

## Key Features

### Administrator Portal
- **Real-Time Dashboard**: Displays aggregate metrics including total invoices, paid invoices, unpaid invoices, pending payment verifications, and total revenue.
- **Invoice Management**: Create, view, edit, filter by status (Paid / Unpaid), search by keywords, and delete invoices (restricted to unpaid invoices without confirmed payments).
- **Customer Directory**: View registered customers, their contact information, registration dates, and associated invoice counts.
- **Payment Verification Workflow**: Review customer payment notifications ("I Have Paid"), inspect invoice details, and either:
  - **Confirm** payment: Updates payment status to *Confirmed* and automatically marks the invoice as *Paid*.
  - **Reject** payment: Requires a mandatory rejection reason, marks payment as *Rejected*, and keeps the invoice *Unpaid*.

### Customer Portal
- **Customer Dashboard**: Overview of personal invoices, total invoiced amounts, outstanding balances, and pending submissions.
- **Strict Data Isolation**: Customers can only view, download, and notify payments for invoices issued specifically to them.
- **Invoice Details & History**: View detailed invoice breakdown and the chronological history of submitted payment verification requests.
- **Payment Notification ("I Have Paid")**: Submit payment execution notifications for unpaid invoices. Duplicate pending submissions for the same invoice are prevented.
- **Rejection Feedback**: If a payment submission is rejected, the customer can view the administrator's reason and resubmit a notification once resolved.

### Core System Features
- **Server-Side PDF Generation**: Generates clean, professional PDF billing documents using QuestPDF. Downloaded invoices exclude internal payment status badges to serve as clean payment documents.
- **Role-Based Access Control (RBAC)**: Secure authorization enforced on both frontend routes (React Router protected route wrappers) and backend API endpoints (`[Authorize(Roles = "Admin")]` and `[Authorize(Roles = "Customer")]`).
- **Secure Public Registration**: Public self-registration assigns the *Customer* role exclusively. Administrative accounts cannot be created through the public registration form.
- **Bilingual Interface (i18n)**: Seamless language switcher in the navigation bar supporting **English (en)** and **Albanian (sq)** with full translation parity across all labels, validation messages, and system alerts. Language preference persists across browser refreshes via `localStorage`.
- **Manual Payment Verification**: Designed around an administrative review model. There is no real bank or third-party payment gateway integration; all confirmations are processed manually by administrators.

---

## Technologies Used

### Backend
- **Framework**: .NET 10.0 (C#) / ASP.NET Core Web API
- **Database & ORM**: SQLite with Entity Framework Core 10.0
- **Authentication**: ASP.NET Core Identity with JWT (JSON Web Token) Bearer authentication
- **Document Generation**: QuestPDF (v2026.9.1)
- **API Documentation**: Scalar OpenAPI (`Scalar.AspNetCore` v2.4.1) accessible at `/scalar/v1`

### Frontend
- **Library**: React 19 (JavaScript, ES Modules)
- **Build Tool**: Vite (v8.3.5)
- **Routing**: React Router DOM (v7)
- **HTTP Client**: Axios with request/response interceptors for JWT token attachment and 401/403 status handling
- **Internationalization**: `i18next`, `react-i18next`, and `i18next-browser-languagedetector`
- **Styling**: Modern pure CSS3 with CSS custom properties, responsive grid/flexbox layouts, and mobile navigation

---

## Screenshots

> Place your application screenshots inside the `screenshots/` directory. Recommended captures:

| Screenshot | Description |
|---|---|
| `screenshots/admin-dashboard.png` | Admin Dashboard with financial statistics and pending payment queue |
| `screenshots/admin-invoices.png` | Admin invoice management table with search and status filtering |
| `screenshots/payment-verification.png` | Admin payment review screen showing confirm and reject options |
| `screenshots/customer-invoices.png` | Customer invoice list with status indicators and "I Have Paid" action |
| `screenshots/invoice-details.png` | Detailed invoice view with payment submission history |
| `screenshots/pdf-invoice.png` | Downloaded QuestPDF invoice document |

---

## Project Structure

```text
InvoiceManagementSystem1/
├── backend/
│   └── InvoiceManagementSystem.API/
│       ├── Controllers/          # API Controllers (Auth, Invoices, Payments, Customers)
│       ├── Data/                 # AppDbContext, DbSeeder
│       ├── DTOs/                 # Request and Response Data Transfer Objects
│       ├── Extensions/           # Service registration extensions (JWT, DB, CORS, OpenAPI)
│       ├── Middleware/           # Global exception handling middleware
│       ├── Migrations/           # Entity Framework Core migrations
│       ├── Models/               # Domain models (ApplicationUser, Invoice, Payment, Enums)
│       ├── Services/             # Business logic (Auth, Invoice, Payment, PDF generation)
│       ├── appsettings.json      # Base application configuration
│       └── InvoiceManagementSystem.API.csproj
│
├── frontend/
│   ├── public/                   # Static assets (favicons, SVG icons)
│   ├── src/
│   │   ├── admin/                # Admin pages (Dashboard, Invoices, Customers, Payments)
│   │   ├── customer/             # Customer pages (Dashboard, Invoices, Details)
│   │   ├── components/           # Reusable UI components (Navbar, Layout, Buttons, Modals)
│   │   ├── context/              # Authentication context (AuthContext)
│   │   ├── locales/              # Translation resource dictionaries (en.json, sq.json)
│   │   ├── pages/                # Public pages (Login, Register, Unauthorized, NotFound)
│   │   ├── services/             # Axios API service modules
│   │   ├── App.jsx               # Route definitions and access control
│   │   └── main.jsx              # React application entry point
│   ├── .env.example              # Environment variables template
│   └── package.json
│
├── screenshots/                  # Application screenshot gallery
├── .gitignore
└── README.md
```

---

## Prerequisites

Before running the project, ensure you have the following installed on your machine:

- **[.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** or higher
- **[Node.js](https://nodejs.org/)** (v18.x, v20.x, or higher) and **npm**
- **Git**
- *Note*: SQLite is embedded via EF Core packages; no external database server installation is needed.

---

## Installation and Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd InvoiceManagementSystem1
```

### 2. Configure and Run Backend

1. Navigate to the backend directory:
   ```bash
   cd backend/InvoiceManagementSystem.API
   ```

2. Create a local development configuration file named `appsettings.Development.json` (this file is excluded by `.gitignore` to prevent committing credentials):
   ```json
   {
     "Jwt": {
       "Key": "YOUR_LONG_SECURE_JWT_SECRET_KEY_MINIMUM_32_CHARACTERS",
       "Issuer": "InvoiceManagementSystem",
       "Audience": "InvoiceManagementSystemUsers",
       "ExpiresInMinutes": 120
     },
     "AdminSeed": {
       "Email": "admin@example.com",
       "Password": "AdminPassword123!",
       "FullName": "System Administrator"
     }
   }
   ```

   > [!NOTE]
   > On application launch, `DbSeeder` automatically creates the `Admin` and `Customer` identity roles. If no user exists with the `AdminSeed:Email` address, it automatically provisions the initial administrator account with the configured credentials. Passwords are securely hashed by ASP.NET Core Identity.

3. Restore dependencies and start the backend API:
   ```bash
   dotnet restore
   dotnet run --launch-profile http
   ```

   The backend will be available at:
   - **API Base URL**: `http://localhost:5000`
   - **Interactive API Documentation (Scalar UI)**: `http://localhost:5000/scalar/v1`
   - **Health Check Endpoint**: `http://localhost:5000/api/health`

### 3. Configure and Run Frontend

1. Open a new terminal window and navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Create a `.env` file based on `.env.example`:
   ```bash
   # On Windows PowerShell:
   Copy-Item .env.example .env

   # On Linux/macOS:
   cp .env.example .env
   ```

   Ensure `.env` contains:
   ```env
   VITE_API_URL=http://localhost:5000
   ```

3. Install frontend dependencies and start the Vite development server:
   ```bash
   npm install
   npm run dev
   ```

   The frontend application will be accessible at:
   - **Frontend URL**: `http://localhost:5173`

---

## How to Test the Application

Follow this end-to-end workflow to test all application capabilities:

1. **Start Applications**: Ensure both the backend (`http://localhost:5000`) and frontend (`http://localhost:5173`) are running.
2. **Register a Customer Account**:
   - Open `http://localhost:5173/register` in your browser.
   - Register a new customer (e.g., `john@example.com`, password with at least 8 characters, digit, uppercase, and lowercase).
   - After registration, you are redirected to the Login page.
3. **Log In as Administrator**:
   - Log in using the credentials defined in your `AdminSeed` configuration (e.g., `admin@example.com`).
   - You will land on the **Admin Dashboard** displaying real-time metrics.
4. **Create an Invoice**:
   - Navigate to **Invoices** > **Create Invoice** (or click "Create Invoice" on the dashboard).
   - Select the newly registered customer from the dropdown.
   - Enter an amount (e.g., `250.00`), issue date, due date, description, and optional notes.
   - Submit the form. The invoice appears in the invoice list with status **Unpaid**.
5. **Log In as the Customer**:
   - Log out of the Admin account and log in with the Customer credentials.
   - On the **Customer Dashboard**, observe that the invoice is reflected in personal statistics.
   - Navigate to **My Invoices** and open the invoice details.
   - Click **Download PDF** to verify the server-generated invoice document.
6. **Submit a Payment Notification**:
   - On the invoice details page, click **I Have Paid**.
   - The invoice state updates with a banner indicating that the payment submission is pending administrator review.
7. **Verify Payment as Administrator**:
   - Log back in as Admin and open the **Payment Verification** page.
   - You will see the pending payment submission under the **Pending** tab.
   - **Scenario A (Confirm)**: Click **Confirm**. The payment is marked as *Confirmed*, and the related invoice status becomes *Paid*.
   - **Scenario B (Reject)**: Click **Reject**, enter a rejection reason (e.g., "Transaction reference not found in bank statement"), and confirm. The payment is marked as *Rejected*, and the invoice remains *Unpaid*.
8. **Check Updated Status as Customer**:
   - Log in as the Customer.
   - If confirmed: The invoice displays the *Paid* status.
   - If rejected: The customer sees the administrator's rejection explanation and is given the option to re-notify once resolved.
9. **Test Bilingual Switching**:
   - Use the language selector (**SQ \| EN**) in the top navigation bar at any time to verify that all interface labels, buttons, tables, and notifications translate immediately between English and Albanian.

---

## Limitations

- **No Live Payment Gateway**: The platform models a manual payment verification workflow (bank transfer / manual settlement). It does not integrate automated payment gateways (e.g., Stripe, PayPal).
- **Single Base Currency**: Currency amounts are formatted in Euros (€) by default.
- **Embedded Database**: Configured with SQLite for straightforward local evaluation without requiring external database provisioning.

---

## Future Improvements

- Integration with payment gateway providers (e.g., Stripe webhooks) for instant online settlements.
- Automated email notifications for invoice issuance, payment submission, and payment approval/rejection.
- Exporting financial reports and invoice records to CSV/Excel.
- Multi-currency support and automated tax/VAT calculations.
- Configurable invoice numbering templates and company branding settings.
