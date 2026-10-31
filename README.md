# Invoice Management System

A full-stack Invoice Management System built with **ASP.NET Core Web
API** and **React**. It provides separate experiences for Administrators
and Customers, server-side PDF invoice generation, manual payment
verification, and a bilingual interface in English and Albanian.

This project was developed as a technical recruitment assignment to
demonstrate full-stack development, secure authentication, role-based
access control, and end-to-end integration.

------------------------------------------------------------------------

## Key Features

### Administrator Portal

-   **Dashboard:** Overview of total invoices, paid and unpaid invoices,
    pending payment verifications, and total revenue.
-   **Invoice Management:** Create, view, edit, search, filter, and
    delete eligible invoices. Invoices with confirmed payments cannot be
    deleted.
-   **Customer Management:** View registered customers, contact details,
    registration dates, and invoice counts.
-   **Payment Verification:** Review customer payment notifications and
    confirm or reject them. Rejection requires a reason.

### Customer Portal

-   **Dashboard:** View personal invoice totals, outstanding balances,
    and pending payment submissions.
-   **My Invoices:** View and download invoices issued to the signed-in
    customer.
-   **Invoice Details & History:** Review invoice information and the
    history of payment verification requests.
-   **Payment Notifications:** Notify the administrator when an invoice
    has been paid. Duplicate pending submissions are prevented.
-   **Rejection Feedback:** View the administrator's rejection reason
    and submit a new notification after resolving the issue.

### Core Features

-   **PDF Generation:** Generate downloadable invoice PDFs on the server
    using QuestPDF.
-   **Role-Based Access Control:** Protect frontend routes and backend
    API endpoints according to Admin and Customer roles.
-   **Secure Registration:** Public registration creates Customer
    accounts only. Admin accounts are provisioned through the backend
    configuration.
-   **Bilingual Interface:** Switch between English and Albanian.
    Language preferences persist across browser refreshes.
-   **Manual Payment Review:** Payments are reviewed and confirmed by an
    administrator. The application does not integrate with a bank or
    external payment gateway.

------------------------------------------------------------------------

## Screenshots

The screenshots below provide an overview of the main application pages.

```{=html}
<table>
```
```{=html}
<tr>
```
```{=html}
<td align="center" width="50%">
```
`<strong>`{=html}Admin Dashboard`</strong>`{=html}`<br/>`{=html}
`<img src="./screenshots/admin-dashboard.png" width="700" height="394" alt="Admin Dashboard"/>`{=html}
```{=html}
</td>
```
```{=html}
<td align="center" width="50%">
```
`<strong>`{=html}Admin Invoices`</strong>`{=html}`<br/>`{=html}
`<img src="./screenshots/admin-invoices.png" width="700" height="394" alt="Admin Invoices"/>`{=html}
```{=html}
</td>
```
```{=html}
</tr>
```
```{=html}
<tr>
```
```{=html}
<td align="center" width="50%">
```
`<strong>`{=html}Create Invoice`</strong>`{=html}`<br/>`{=html}
`<img src="./screenshots/create-invoices.png" width="700" height="394" alt="Create Invoice"/>`{=html}
```{=html}
</td>
```
```{=html}
<td align="center" width="50%">
```
`<strong>`{=html}Customer Invoices`</strong>`{=html}`<br/>`{=html}
`<img src="./screenshots/customer-invoices.png" width="700" height="394" alt="Customer Invoices"/>`{=html}
```{=html}
</td>
```
```{=html}
</tr>
```
```{=html}
<tr>
```
```{=html}
<td align="center" width="50%">
```
`<strong>`{=html}Invoice Details`</strong>`{=html}`<br/>`{=html}
`<img src="./screenshots/invoice-details.png" width="700" height="394" alt="Invoice Details"/>`{=html}
```{=html}
</td>
```
```{=html}
<td align="center" width="50%">
```
`<strong>`{=html}PDF Invoice`</strong>`{=html}`<br/>`{=html}
`<img src="./screenshots/pdf-invoice.png" width="700" height="394" alt="PDF Invoice"/>`{=html}
```{=html}
</td>
```
```{=html}
</tr>
```
```{=html}
<tr>
```
```{=html}
<td align="center" width="50%">
```
`<strong>`{=html}Manage Customers`</strong>`{=html}`<br/>`{=html}
`<img src="./screenshots/manage_customers.png" width="700" height="394" alt="Manage Customers"/>`{=html}
```{=html}
</td>
```
```{=html}
<td width="50%">
```
```{=html}
</td>
```
```{=html}
</tr>
```
```{=html}
</table>
```

------------------------------------------------------------------------

## Technologies Used

### Backend

-   **Framework:** .NET 10.0 / ASP.NET Core Web API
-   **Database & ORM:** SQLite with Entity Framework Core 10.0
-   **Authentication:** ASP.NET Core Identity with JWT Bearer
    authentication
-   **PDF Generation:** QuestPDF
-   **API Documentation:** Scalar OpenAPI

### Frontend

-   **Library:** React 19 (JavaScript, ES Modules)
-   **Build Tool:** Vite
-   **Routing:** React Router DOM
-   **HTTP Client:** Axios with request/response interceptors
-   **Internationalization:** i18next and react-i18next
-   **Styling:** CSS3 with custom properties and responsive layouts

------------------------------------------------------------------------

## Project Structure

``` text
InvoiceManagementSystem1/
├── backend/
│   └── InvoiceManagementSystem.API/
│       ├── Controllers/
│       ├── Data/
│       ├── DTOs/
│       ├── Extensions/
│       ├── Middleware/
│       ├── Migrations/
│       ├── Models/
│       ├── Services/
│       ├── appsettings.json
│       └── InvoiceManagementSystem.API.csproj
├── frontend/
│   ├── public/
│   ├── src/
│   │   ├── admin/
│   │   ├── customer/
│   │   ├── components/
│   │   ├── context/
│   │   ├── locales/
│   │   ├── pages/
│   │   ├── services/
│   │   ├── App.jsx
│   │   └── main.jsx
│   ├── .env.example
│   └── package.json
├── screenshots/
├── .gitignore
└── README.md
```

------------------------------------------------------------------------

## Prerequisites

Make sure the following tools are installed:

-   [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
    or later
-   [Node.js](https://nodejs.org/) (v18.x, v20.x, or later) and npm
-   Git

**Database:** SQLite is included through the Entity Framework Core
packages. No separate database server is required.

------------------------------------------------------------------------

## Installation and Setup

### 1. Clone the Repository

``` bash
git clone <repository-url>
cd InvoiceManagementSystem1
```

### 2. Configure and Run the Backend

Navigate to the backend project:

``` bash
cd backend/InvoiceManagementSystem.API
```

Create an `appsettings.Development.json` file in the backend project
directory. This local file is excluded from version control so that
development secrets are not committed.

Use the following structure and replace the example values with your own
secure credentials:

``` json
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

On startup, `DbSeeder` creates the Admin and Customer roles. If an
administrator with the configured email does not already exist, the
application creates the initial Admin account. ASP.NET Core Identity
securely hashes account passwords.

Restore dependencies and start the API:

``` bash
dotnet restore
dotnet run --launch-profile http
```

The backend runs at:

-   **API:** `http://localhost:5000`
-   **Scalar API Documentation:** `http://localhost:5000/scalar/v1`
-   **Health Check:** `http://localhost:5000/api/health`

### 3. Configure and Run the Frontend

Open a second terminal and navigate to the frontend directory from the
repository root:

``` bash
cd frontend
```

Create a `.env` file from the example:

**Windows PowerShell**

``` powershell
Copy-Item .env.example .env
```

**Linux / macOS**

``` bash
cp .env.example .env
```

Set the API URL in `.env`:

``` env
VITE_API_URL=http://localhost:5000
```

Install dependencies and start the frontend:

``` bash
npm install
npm run dev
```

The frontend runs at `http://localhost:5173`.

------------------------------------------------------------------------

## How to Test the Application

Make sure both the backend and frontend are running before testing.

### 1. Create a Customer Account

1.  Open `http://localhost:5173/register`.
2.  Register a customer account using a valid email and password.
3.  Return to the login page.

### 2. Log In as Admin and Create an Invoice

1.  Log in using the credentials configured under `AdminSeed`.
2.  Open **Invoices** and select **Create Invoice**.
3.  Select the customer you registered.
4.  Enter the invoice amount, issue date, due date, description, and any
    optional notes.
5.  Submit the form. The invoice should appear with an **Unpaid**
    status.

### 3. Test the Customer Workflow

1.  Log out of the Admin account and log in using the Customer account.
2.  Open **My Invoices** and select the invoice.
3.  Review the invoice details and download the PDF.
4.  Select **I Have Paid** to submit a payment notification.

### 4. Verify the Payment as Admin

1.  Log back in as Admin and open **Payment Verification**.
2.  Find the pending customer submission.
3.  Test either action:
    -   **Confirm:** The payment becomes Confirmed and the invoice
        status changes to Paid.
    -   **Reject:** Enter a rejection reason. The payment becomes
        Rejected and the invoice remains Unpaid.

### 5. Check the Result as Customer

Log in as the Customer again: - If the payment was confirmed, the
invoice should show **Paid**. - If it was rejected, the customer can
view the rejection reason and submit a new payment notification after
resolving the issue.

### 6. Test Language Switching

Use the language selector (**SQ / EN**) in the navigation bar to switch
between Albanian and English. Check that page labels, buttons,
validation messages, and notifications update accordingly.

------------------------------------------------------------------------

## Important Notes

-   Payment verification is a manual administrative process. No real
    bank or third-party payment gateway is connected.
-   Admin accounts are created through the backend seeding
    configuration, not through public registration.
-   Use your own local development credentials. Do not commit
    `appsettings.Development.json` or other files containing secrets.
