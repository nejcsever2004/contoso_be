# Contoso University Backend

## Overview

The **Contoso University Backend** project is a web-based application designed to manage student enrollments, courses, and academic records for a fictional university. Built with **ASP.NET Core** and **Entity Framework Core**, it follows the **Razor Pages** approach for rendering UI while offering **RESTful API endpoints** for external access. The backend is structured to handle authentication, authorization, and data persistence efficiently using **SQLite**.

---

## Technologies Used

- **Programming Language**: C#
- **Framework**: ASP.NET Core with Razor Pages
- **Database**: SQLite (Entity Framework Core)
- **Identity Management**: ASP.NET Core Identity
- **IDE**: Visual Studio
- **ORM (Object-Relational Mapping)**: Entity Framework Core
- **Logging**: Built-in ASP.NET Core logging framework

---

## Project Structure

The solution is organized into several key directories:

- **Controllers**: Houses API controllers responsible for handling HTTP requests.
- **Data**: Contains the database context (`SchoolContext.cs`) and migration files for database schema management.
- **Models**: Defines the data structures and entities used throughout the application.
- **Pages**: Implements UI using **Razor Pages**, with corresponding `.cshtml` and `.cshtml.cs` files.
- **wwwroot**: Stores static assets like CSS, JavaScript, and images.

---

## Models

The **Models** directory contains C# classes that represent database entities and business logic.

### **Student.cs**
Represents a university student.

#### Properties:
- `StudentID` (**int**, primary key) – Unique identifier.
- `LastName` (**string**) – Last name of the student.
- `FirstMidName` (**string**) – First or middle name of the student.
- `EnrollmentDate` (**DateTime**) – Date when the student enrolled.

#### Relationships:
- One-to-many with `Enrollment` (a student can have multiple enrollments).

---

### **Course.cs**
Defines university courses.

#### Properties:
- `CourseID` (**int**, primary key) – Unique identifier.
- `Title` (**string**) – Course name.
- `Credits` (**int**) – Number of credits awarded for the course.

#### Relationships:
- One-to-many with `Enrollment` (a course can have multiple enrollments).

---

### **Enrollment.cs**
Represents a student's enrollment in a course.

#### Properties:
- `EnrollmentID` (**int**, primary key) – Unique identifier.
- `CourseID` (**int**, foreign key) – Links to a `Course`.
- `StudentID` (**int**, foreign key) – Links to a `Student`.
- `Grade` (**enum?, nullable**) – Student's grade in the course.

#### Relationships:
- Many-to-one with `Student` and `Course`.

---

## Controllers

The **Controllers** directory manages HTTP request handling and business logic execution.

### **GradesAndScheduleController.cs**
Handles student academic records and schedules.

#### Endpoints:
- `GET /api/grades/{studentId}` – Retrieves grades for a specific student.
- `GET /api/schedule/{studentId}` – Fetches class schedule for a student.

#### Usage:
- Enables API calls to fetch academic information, primarily for frontend integration or third-party applications.

---

## Authentication Models

The application supports **user authentication and registration** via ASP.NET Core Identity.

### **RegisterModel.cs**
Handles user registration.

#### Properties:
- `Input.Email` – User email.
- `Input.Password` – Account password.
- `Input.ConfirmPassword` – Password confirmation field.

#### Methods:
- `OnPostAsync()` – Processes registration form submissions, validates inputs, and creates a new user in the database.

---

### **LoginModel.cs**
Manages user login.

#### Properties:
- `Input.Email` – User email.
- `Input.Password` – Account password.
- `ReturnUrl` – Redirect URL after login.

#### Methods:
- `OnPostAsync()` – Authenticates user credentials and starts a session.

---

## Database Context

The `SchoolContext.cs` file, located in the **Data** directory, defines the application's database interactions. It uses **Entity Framework Core** to map models to database tables.

#### Key configurations in `SchoolContext.cs`:
```csharp
public class SchoolContext : DbContext
{
    public SchoolContext(DbContextOptions<SchoolContext> options) : base(options) {}

    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
}
API Endpoints
The backend exposes RESTful API endpoints for external interaction.

HTTP Method	Endpoint	Description
GET	/api/students	Retrieves all students
GET	/api/students/{id}	Fetches details of a student
POST	/api/students	Creates a new student
PUT	/api/students/{id}	Updates a student's record
DELETE	/api/students/{id}	Deletes a student
GET	/api/courses	Retrieves all courses
POST	/api/enrollments	Enrolls a student in a course

Authentication & Authorization
User Authentication: Implemented using ASP.NET Core Identity.

Authorization: Enforced via [Authorize] attributes.

Role-Based Access Control (RBAC): Future implementation includes Admin, Student, Instructor roles.

Key Authentication Flow:
Users register using RegisterModel.cs.

Credentials are securely stored in the SQLite database.

Users log in via LoginModel.cs, obtaining an authentication cookie/token.

API endpoints are protected via authentication middleware.

