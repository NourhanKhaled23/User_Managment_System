# **User Management System (UMS)** 🚀

## **Overview**
The **User Management System (UMS)** is a backend service built with **ASP.NET Core** and **Entity Framework Core**. It handles essential user operations such as:
- **User Registration & Login** (with JWT Authentication)
- **Profile Management** (view, update, delete)
- **Role Management** (Admin, Instructor, Student)
- **Password Management** (change password)
- **Admin Operations** (view all users, update roles, delete users)

This project follows **Clean Architecture**, ensuring modularity and scalability.

---

## **📌 Features**
- ✅ Secure **JWT-based Authentication**
- ✅ **BCrypt Password Hashing** for security
- ✅ **Role-Based Access Control (RBAC)**
- ✅ **Admin User Management**
- ✅ **Swagger API Documentation**
- ✅ **Structured Logging with Serilog**

---

## **🛠 Installation**
### **1️⃣ Clone the Repository**
```bash
git clone https://github.com/NourhanKhaled23/User_Managment_System.git
cd User_Managment_System
```

### **2️⃣ Set Up the Database**
1. **Update `appsettings.json`** with your SQL Server connection string.
2. Run database migrations:
   ```bash
   dotnet ef database update
   ```

### **3️⃣ Run the Application**
```bash
dotnet run
```

- The API will be available at **`https://localhost:7233`**.
- Open **Swagger UI**: `https://localhost:7233/swagger`

---

## **🔗 API Endpoints**
### **Authentication**
#### 🆕 Register User
**POST /api/Auth/register**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "password123"
}
```
#### 🔐 Login User
**POST /api/Auth/login**
```json
{
  "email": "john.doe@example.com",
  "password": "password123"
}
```
#### 🔍 Get User Profile
**GET /api/User/profile**

#### 🔄 Update Profile
**PUT /api/User/profile**
```json
{
  "firstName": "Jane",
  "lastName": "Smith"
}
```

#### 🔑 Change Password
**PUT /api/User/change-password**
```json
{
  "oldPassword": "password123",
  "newPassword": "newpassword123"
}
```
#### 🗑 Delete Account
**DELETE /api/User/delete-account**

### **Admin Endpoints**
#### 👥 View All Users
**GET /api/Admin/users**
#### 🔄 Change User Role
**PUT /api/Admin/update-role**
```json
{
  "userId": 1,
  "role": "Instructor"
}
```
#### ❌ Delete User
**DELETE /api/Admin/delete-user**
```json
{
  "userId": 1
}
```

---

## **📂 Project Structure**
```plaintext
📁 UserManagementSystem
 ┣ 📁 Domain            # Core business logic
 ┣ 📁 Application       # Services & business rules
 ┣ 📁 Infrastructure    # Database & security
 ┣ 📁 UserManagementAPI # API controllers & middleware
 ┣ 📁 Tests             # Unit & integration tests
 ┣ 📄 README.md         # Project documentation
 ┣ 📄 appsettings.json  # Configuration file
 ┗ 📄 Program.cs        # API entry point
```

---


## **🚀 Best Practices**
- **Use HTTPS** for secure API calls.
- **Enable API versioning** to avoid breaking changes.
- **Write unit tests** using xUnit & Moq.
- **Use structured logging** for debugging.

---

## **🎯 Conclusion**
This **User Management System** provides a secure, scalable backend for managing user authentication and roles. Feel free to contribute or suggest improvements!

**📧 Contact:** If you have any questions, open an issue or reach out! 🚀
