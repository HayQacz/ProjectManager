# 1. API Controllers – Documentation

## 1.1 UsersController

**Base Route:** `api/Users`  
**Description:** Handles user registration, authentication (login), and retrieval.

### Endpoints:

| Method | Route       | Auth Required | Description                        |
|--------|-------------|---------------|------------------------------------|
| POST   | `/register` | ❌ No          | Registers a new user               |
| POST   | `/login`    | ❌ No          | Authenticates user and returns JWT |
| GET    | `/{id}`     | ✅ Yes         | Retrieves user data by ID          |

---

## 1.2 ProjectsController

**Base Route:** `api/Projects`  
**Description:** Handles CRUD operations for projects.

### Endpoints:

| Method | Route   | Auth Required | Description                   |
|--------|---------|---------------|-------------------------------|
| POST   | `/`     | ✅ Yes         | Creates a new project         |
| GET    | `/{id}` | ✅ Yes         | Retrieves a project by its ID |
| GET    | `/`     | ✅ Yes         | Retrieves all projects        |
| PUT    | `/{id}` | ✅ Yes         | Updates an existing project   |
| DELETE | `/{id}` | ✅ Yes         | Deletes a project             |

---

## 1.3 ProjectMembersController

**Base Route:** `api/Projects/{projectId}/ProjectMembers`  
**Description:** Manages project members – adding, removing, and role changes.

### Endpoints:

| Method | Route                   | Auth Required | Description                          |
|--------|-------------------------|---------------|--------------------------------------|
| POST   | `/`                     | ✅ Yes         | Adds a new project member            |
| POST   | `/add/{userId}`         | ✅ Yes         | Adds an existing user to the project |
| DELETE | `/remove/{userId}`      | ✅ Yes         | Removes a user from the project      |
| PUT    | `/change-role/{userId}` | ✅ Yes         | Changes the role of a project member |
| GET    | `/{userId}`             | ✅ Yes         | Retrieves a specific member by ID    |
| GET    | `/`                     | ✅ Yes         | Retrieves all members of the project |

---

## 1.4 ProjectTasksController

**Base Route:** `api/Projects/{projectId}/ProjectTasks`  
**Description:** Handles project task management – filtering, adding, updating, deleting.

### Endpoints:

| Method | Route       | Auth Required | Description                                                |
|--------|-------------|---------------|------------------------------------------------------------|
| GET    | `/`         | ✅ Yes         | Retrieves tasks with optional filters (status, assignment) |
| POST   | `/`         | ✅ Yes         | Creates a new task for the project                         |
| PUT    | `/{taskId}` | ✅ Yes         | Updates a specific task                                    |
| DELETE | `/{taskId}` | ✅ Yes         | Deletes a specific task                                    |

---

## 1.5 Endpoints Overview Table

| Controller                   | Method | Endpoint                                                        | Description                    |
|------------------------------|--------|-----------------------------------------------------------------|--------------------------------|
| **UsersController**          | POST   | `/api/Users/register`                                           | Registers a new user           |
|                              | POST   | `/api/Users/login`                                              | Authenticates user             |
|                              | GET    | `/api/Users/{id}`                                               | Gets user data by ID           |
| **ProjectsController**       | POST   | `/api/Projects`                                                 | Creates a new project          |
|                              | GET    | `/api/Projects`                                                 | Retrieves all projects         |
|                              | GET    | `/api/Projects/{id}`                                            | Gets a project by ID           |
|                              | PUT    | `/api/Projects/{id}`                                            | Updates a project              |
|                              | DELETE | `/api/Projects/{id}`                                            | Deletes a project              |
| **ProjectMembersController** | POST   | `/api/Projects/{projectId}/ProjectMembers`                      | Adds a new member to a project |
|                              | POST   | `/api/Projects/{projectId}/ProjectMembers/add/{userId}`         | Adds user to a project         |
|                              | DELETE | `/api/Projects/{projectId}/ProjectMembers/remove/{userId}`      | Removes user from project      |
|                              | PUT    | `/api/Projects/{projectId}/ProjectMembers/change-role/{userId}` | Changes member's role          |
|                              | GET    | `/api/Projects/{projectId}/ProjectMembers`                      | Gets all members in a project  |
|                              | GET    | `/api/Projects/{projectId}/ProjectMembers/{userId}`             | Gets a member by ID            |
| **ProjectTasksController**   | GET    | `/api/Projects/{projectId}/ProjectTasks`                        | Retrieves tasks (with filters) |
|                              | POST   | `/api/Projects/{projectId}/ProjectTasks`                        | Adds a new task                |
|                              | PUT    | `/api/Projects/{projectId}/ProjectTasks/{taskId}`               | Updates a task                 |
|                              | DELETE | `/api/Projects/{projectId}/ProjectTasks/{taskId}`               | Deletes a task                 |

# 2. Persistence – AppDbContext

## 2.1 Overview

The `AppDbContext` is the Entity Framework Core database context for the application. It defines the schema and relationships of all major domain entities.

### Defined DbSets:

```csharp
public DbSet<Project> Projects
public DbSet<ProjectDetails> ProjectDetails
public DbSet<ProjectMember> ProjectMembers
public DbSet<ProjectCategory> ProjectCategories
public DbSet<ProjectCategoryItem> ProjectCategoryItems
public DbSet<User> Users
public DbSet<ProjectTask> ProjectTasks
```

## 2.2 Model Configuration

The `OnModelCreating` method is overridden to configure entity relationships and behaviors:

### Configurations:

- **Project ↔ ProjectDetails**
    - One-to-one relationship
    - Cascade delete

- **Project ↔ ProjectMembers**
    - Many-to-many relationship using a join table `ProjectMembersProjects`

- **User ↔ ProjectMember**
    - One-to-one relationship
    - Uses `ProjectMemberId` as foreign key in `User`

- **Project ↔ ProjectTask**
    - One-to-many relationship
    - Cascade delete

- **ProjectTask ↔ AssignedMember**
    - Many-to-one relationship (nullable)
    - Set null on delete

This context is registered and injected via `DbContextOptions<AppDbContext>` and used across the application for data access and entity persistence.

# 3. Services

## 3.1 Authorization

### 3.1.1 ProjectAuthorizationService

The `ProjectAuthorizationService` implements `IProjectAuthorizationService` and provides logic to evaluate whether a user has access to view, edit, delete a project or manage its members. It uses `AppDbContext` to query the database and validate roles and relationships.

#### Main Methods:

- `CanViewProject(Guid userId, Guid projectId)`
    - Allows access if the project is public or the user is a member with a valid role (Contributor, Manager, Owner).

- `CanEditProject(Guid userId, Guid projectId)`
    - Grants edit permissions if the user is an Owner or Manager in the project.

- `CanDeleteProject(Guid userId, Guid projectId)`
    - Restricts deletion to project Owners only.

- `CanManageMembers(Guid userId, Guid projectId)`
    - Validates if the user has privileges (Manager or Owner) to manage project members.

---

## 3.2 Implementations

### 3.2.1 UserContext

The `UserContext` class implements the `IUserContext` interface. It provides access to the currently authenticated user ID based on HTTP context claims.

#### Key Property:

- `UserId`
    - Extracted from `ClaimTypes.NameIdentifier` within the current HTTP context.
    - Throws `UnauthorizedAccessException` if no user claim is present.

---

## 3.3 Interfaces

### 3.3.1 IProjectAuthorizationService

```csharp
public interface IProjectAuthorizationService
{
    Task<bool> CanViewProject(Guid userId, Guid projectId);
    Task<bool> CanEditProject(Guid userId, Guid projectId);
    Task<bool> CanDeleteProject(Guid userId, Guid projectId);
    Task<bool> CanManageMembers(Guid userId, Guid projectId);
}
```

Provides a contract for implementing project-based authorization rules.

---

### 3.3.2 IUserContext

```csharp
public interface IUserContext
{
    Guid UserId { get; }
}
```

Minimal interface for obtaining the authenticated user's ID from the current request context.

# 4. Features

## 4.1 Projects

### 4.1.1 Commands

#### 4.1.1.1 CreateProjectCommand
Defines a command to create a new project and optionally assign its details.

- **Returns:** `Guid` of the created project
- **Handler:** `CreateProjectHandler`
- **Persistence:** Adds a `Project` and optionally its `ProjectDetails` to the database

#### 4.1.1.2 UpdateProjectCommand
Command to update project fields and its related `ProjectDetails`.

- **Returns:** `bool` (true if update was successful)
- **Handler:** `UpdateProjectHandler`
- **Authorization:** Only accessible to Managers and Owners

#### 4.1.1.3 DeleteProjectCommand
Command to delete an existing project, including its details and relations.

- **Returns:** `bool` (true if deletion was successful)
- **Handler:** `DeleteProjectHandler`
- **Authorization:** Restricted to Managers and Owners

---

### 4.1.2 Queries

#### 4.1.2.1 GetAllProjectsQuery
Retrieves a list of projects that the current user has access to view (public or member).

- **Returns:** `List<ProjectDto>`
- **Handler:** `GetAllProjectsHandler`
- **Filtering:** Checks project visibility and user membership

#### 4.1.2.2 GetProjectByIdQuery
Retrieves a single project by ID, if the user has permission to view it.

- **Returns:** `ProjectDto?`
- **Handler:** `GetProjectByIdHandler`
- **Authorization:** Uses `IProjectAuthorizationService`

---

### 4.1.3 Models

#### 4.1.3.1 ProjectDto
DTO used to transfer project data.

```csharp
public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public ProjectStatus? Status { get; set; }
    public string? TechnologiesUsed { get; set; }
    public bool? IsCommercial { get; set; }
    public ProjectVisibility? Visibility { get; set; }
}
```

---

## 4.2 Users

### 4.2.1 Commands

#### 4.2.1.1 RegisterUserCommand
Handles registration of a new user by hashing their password and saving the user.

- **Returns:** `UserDto`
- **Handler:** `RegisterUserHandler`
- **Security:** Uses `IPasswordHasher<User>` for secure password hashing

#### 4.2.1.2 LoginUserCommand
Handles user authentication by verifying the hashed password and issuing a JWT.

- **Returns:** `string` (JWT token)
- **Handler:** `LoginUserHandler`
- **Security:** Uses `JwtSecurityTokenHandler` and symmetric signing key

### 4.2.2 Queries

#### 4.2.2.1 GetUserByIdQuery
Retrieves a single user by ID and maps it to a DTO.

- **Returns:** `UserDto?`
- **Handler:** `GetUserByIdHandler`

### 4.2.3 Models

#### 4.2.3.1 UserDto
DTO used for exposing user information safely.

```csharp
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid? ProjectMemberId { get; set; }
}
```

---

## 4.3 ProjectMembers

### 4.3.1 Commands

#### 4.3.1.1 CreateProjectMemberCommand
Creates a project member entity with a specific role.

- **Returns:** `ProjectMemberDto`
- **Handler:** `CreateProjectMemberHandler`

#### 4.3.1.2 AddProjectMemberToProjectCommand
Adds an existing project member to a project, if requester is authorized.

- **Returns:** `bool`
- **Handler:** `AddProjectMemberToProjectHandler`
- **Authorization:** Requires Manager or Owner

#### 4.3.1.3 RemoveProjectMemberFromProjectCommand
Removes a project member from the given project.

- **Returns:** `bool`
- **Handler:** `RemoveProjectMemberFromProjectHandler`
- **Authorization:** Requires Manager or Owner

#### 4.3.1.4 ChangeProjectMemberRoleCommand
Updates a member's role within a project.

- **Returns:** `ProjectMemberDto?`
- **Handler:** `ChangeProjectMemberRoleHandler`
- **Authorization:** Requires Manager or Owner

### 4.3.2 Queries

#### 4.3.2.1 GetProjectMembersQuery
Retrieves all members of a given project.

- **Returns:** `List<ProjectMemberDto>`
- **Handler:** `GetProjectMembersHandler`

#### 4.3.2.2 GetProjectMemberByIdQuery
Gets a specific project member by their ID.

- **Returns:** `ProjectMemberDto?`
- **Handler:** `GetProjectMemberByIdHandler`

### 4.3.3 Models

#### 4.3.3.1 ProjectMemberDto
DTO representing a project member.

```csharp
public class ProjectMemberDto
{
    public Guid Id { get; set; }
    public ProjectMemberRole Role { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
}
```

---

## 4.4 ProjectTasks

### 4.4.1 Commands

#### 4.4.1.1 CreateProjectTaskCommand
Creates a new task in a project with optional assignment.

- **Returns:** `ProjectTaskDto`
- **Handler:** `CreateProjectTaskHandler`
- **Authorization:** Requires edit permission for the project

#### 4.4.1.2 UpdateProjectTaskCommand
Updates an existing task with new values.

- **Returns:** `bool`
- **Handler:** `UpdateProjectTaskHandler`
- **Authorization:** Requires edit permission for the project

#### 4.4.1.3 DeleteProjectTaskCommand
Deletes a task by ID.

- **Returns:** `bool`
- **Handler:** `DeleteProjectTaskHandler`
- **Authorization:** Requires edit permission for the project

### 4.4.2 Queries

#### 4.4.2.1 GetProjectTasksQuery
Retrieves a list of tasks from a project with optional filters.

- **Returns:** `List<ProjectTaskDto>`
- **Handler:** `GetProjectTasksHandler`
- **Filters:** By status, assigned to current user, or unassigned
- **Authorization:** Requires membership in the project

### 4.4.3 Models

#### 4.4.3.1 ProjectTaskDto
DTO representing a project task.

```csharp
public class ProjectTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssignedMemberId { get; set; }
}
```

