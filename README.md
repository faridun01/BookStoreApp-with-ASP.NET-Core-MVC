# BookStoreApp

## Overview
BookStoreApp is a web-based application built using **ASP.NET Core MVC** with **Entity Framework Core** for managing books in an online bookstore. The platform enables users to upload, download, and manage books.

## Features

### Book Management
- Upload and store books with metadata (Title, Author, Description).
- Download books with a tracking feature for download count.
- Dynamically generate file paths and store them securely.

### Authentication & Authorization
- User registration and login using **ASP.NET Core Identity**.
- Admin role to manage books and user privileges.
- Role-based access control to restrict certain features to admins.

### Database Integration
- Integrated with **SQL Server** using **Entity Framework Core**.
  
### File Uploads
- Upload PDF files for books.
- Files are stored in the `wwwroot/uploads/books` directory.

### Error Handling
- Provides detailed error messages for debugging during development.
- Validates input fields and file uploads.

## Technologies Used
- **Framework**: ASP.NET Core MVC
- **Database**: SQL Server (using Entity Framework Core)
- **Authentication**: ASP.NET Core Identity
- **Languages**: C#, HTML, CSS
- **Tools**: Visual Studio, .NET CLI
