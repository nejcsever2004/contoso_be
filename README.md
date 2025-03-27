# Contoso University Backend Documentation

## Overview

This document provides an in-depth overview of the Contoso University Backend project, detailing the technologies utilized, the structure of the project, and comprehensive explanations of its components, including models and controllers.

## Technologies Used

- **Programming Language**: C#
- **Framework**: ASP.NET Core with Razor Pages
- **Database**: SQLite
- **IDE**: Visual Studio

## Project Structure

The solution comprises several key directories:

- **Controllers**: Contains the application's API controllers.
- **Data**: Holds the database context and migration files.
- **Models**: Includes the entity classes representing the application's data structures.
- **Pages**: Contains Razor Pages for the application's UI.
- **wwwroot**: Stores static files such as CSS, JavaScript, and images.

## Models

The **Models** directory defines the core data structures used throughout the application. Below is an overview of each model:

### Student.cs
- **Properties**:
  - `StudentID`: Primary key.
  - `LastName`: Student's last name.
  - `FirstMidName`: Student's first or middle name.
  - `EnrollmentDate`: Date the student enrolled.
- **Relationships**:
  - One-to-many with `Enrollment` (a student can have multiple enrollments).

### Course.cs
- **Properties**:
  - `CourseID`: Primary key.
  - `Title`: Course title.
  - `Credits`: Number of credits awarded for the course.
- **Relationships**:
  - One-to-many with `Enrollment` (a course can have multiple enrollments).

### Enrollment.cs
- **Properties**:
  - `EnrollmentID`: Primary key.
  - `CourseID`: Foreign key referencing `Course`.
  - `StudentID`: Foreign key referencing `Student`.
  - `Grade`: Nullable grade achieved by the student in the course.
- **Relationships**:
  - Many-to-one with `Student` and `Course`.

## Controllers

The **Controllers** directory contains classes that handle HTTP requests and responses. Below is an overview of the primary controller:

### GradesAndScheduleController.cs
- **Endpoints**:
  - `GetGrades(int studentId)`: Retrieves the grades for a specific student.
  - `GetSchedule(int studentId)`: Retrieves the class schedule for a specific student.
- **Usage**:
  - Facilitates API calls to fetch academic information for students.

## Authentication Models

The project implements authentication features, including login and registration functionalities. Below is an overview of the related models:

### RegisterModel.cs
- **Properties**:
  - `Input`: Contains fields like `Email`, `Password`, and `ConfirmPassword`.
- **Methods**:
  - `OnPostAsync()`: Processes the registration form submission.
- **Usage**:
  - Validates user input and creates a new user account in the system.

### LoginModel.cs
- **Properties**:
  - `Input`: Contains fields like `Email` and `Password`.
  - `ReturnUrl`: Specifies the URL to redirect to after a successful login.
- **Methods**:
  - `OnPostAsync()`: Processes the login form submission.
- **Usage**:
  - Authenticates users and establishes a session for authorized access.

## Database Context

The **Data** directory contains the `SchoolContext.cs` file, which serves as the database context for the application. It manages entity configurations and database interactions using Entity Framework Core.

## Razor Pages

The **Pages** directory contains Razor Pages that define the UI of the application. Each `.cshtml` file represents a page, with accompanying `.cshtml.cs` files containing page-specific logic.

## Static Files

The **wwwroot** directory holds static files such as CSS, JavaScript, and images, which are served directly to clients.

## Configuration

The application settings are managed in the `appsettings.json` and `appsettings.Development.json` files, which contain configuration data such as connection strings and logging settings.

## Program Entry Point

The application starts execution from `Program.cs`, which configures the ASP.NET Core application pipeline and services.

### Key Components in `Program.cs`
- `var builder = WebApplication.CreateBuilder(args);`
  - Initializes the application with default configurations.
- `builder.Services.AddDbContext<SchoolContext>(options => options.UseSqlite(...));`
  - Configures Entity Framework Core with SQLite as the database provider.
- `app.UseAuthentication();`
  - Enables authentication middleware.
- `app.UseAuthorization();`
  - Enables authorization policies.
- `app.MapRazorPages();`
  - Maps Razor Pages for handling requests.
- `app.Run();`
  - Starts the application.

## API Endpoints Overview

| HTTP Method | Endpoint                | Description |
|------------|------------------------|-------------|
| **GET**    | `/api/students`         | Retrieves a list of all students |
| **GET**    | `/api/students/{id}`    | Fetches details of a specific student |
| **POST**   | `/api/students`         | Creates a new student entry |
| **PUT**    | `/api/students/{id}`    | Updates student details |
| **DELETE** | `/api/students/{id}`    | Deletes a student record |
| **GET**    | `/api/courses`          | Retrieves all courses |
| **POST**   | `/api/enrollments`      | Enrolls a student in a course |

## Authentication & Authorization

- Users register using the **RegisterModel**.
- Upon successful registration, credentials are stored in the database.
- Users log in using **LoginModel**, which validates credentials.
- An authentication cookie or token is issued upon login.
- Protected API endpoints require an authenticated user.

### Authorization Policies
- `Authorize` attributes are used to restrict access to specific Razor Pages and API endpoints.
- Roles such as **Admin**, **Student**, and **Instructor** can be defined for granular access control.

## Error Handling & Logging

### Error Handling
- Uses built-in ASP.NET Core exception handling middleware.
- Custom error pages can be configured in the `Pages/Error.cshtml` file.

### Logging
- Configured in `Program.cs` with `builder.Logging.AddConsole();`
- Logs are recorded in the console and can be extended with file logging.

## Deployment Instructions

1. **Clone the Repository**:
   ```sh
   git clone https://github.com/nejcsever2004/contoso_be.git
   cd contoso_be
   ```
2. **Install Dependencies**:
   ```sh
   dotnet restore
   ```
3. **Apply Database Migrations**:
   ```sh
   dotnet ef database update
   ```
4. **Run the Application**:
   ```sh
   dotnet run
   ```
5. **Access the Application**:
   - Open `https://localhost:5001` in a web browser.

## Future Enhancements

- Implement **JWT authentication** for API security.
- Add **unit and integration tests** using xUnit or NUnit.
- Improve **UI/UX design** for Razor Pages.
- Expand **API functionalities** for better data manipulation.

---

This documentation provides an extensive overview of the **Contoso University Backend** project. 🚀
