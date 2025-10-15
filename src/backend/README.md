# 🧩 Hermes Backend — Database Models & Scaffolding

Welcome to the **Hermes Backend** database guide!
Follow these steps to update, scaffold, and migrate your database models efficiently.

---

## ⚡ Update Database Models

Keeping your database models in sync with your schema is crucial for smooth backend development.

### 🛠️ Prerequisites

Make sure you have the **Entity Framework CLI** installed globally:

```bash
dotnet tool install --global dotnet-ef
```

---

## 💻 Using PowerShell (Windows)

1. Navigate to the **Data** directory:

```powershell
cd src/backend/Hermes.SituationRoom.Data
```

2. Run the scaffolding script:

```powershell
./scaffold.ps1
```

This will update the database models automatically.

---

## 🐚 Using Shell Script (Linux / macOS)

1. Navigate to the **Data** directory:

```bash
cd src/backend/Hermes.SituationRoom.Data
```

2. Make the script executable:

```bash
chmod +x scaffold.sh
```

3. Run the scaffolding script:

```bash
./scaffold.sh
```

This ensures your models match the latest database schema.

---

## 🚀 Database Migrations

To make changes to the database schema, you need to write a **migration**.
We use **Evolve** for database migrations.

* Migrations can be found under:

```
src/backend/Hermes.SituationRoom.Data/Migrations
```

The migrations will automatically apply when you start the backend. Use the scaffold script to update your code, ensuring that both entities and the database context reflect the latest schema changes.

---

✨ *Happy coding! Keep your models and database schema clean and up-to-date with Hermes Backend.*
