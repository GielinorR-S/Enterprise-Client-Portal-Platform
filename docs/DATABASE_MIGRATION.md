# Database Migration Documentation

## InitialCreate Migration

**Migration Name:** `20251203022547_InitialCreate`  
**Location:** `src/HelixPortal.Infrastructure/Migrations/`  
**Status:** ✅ Created and ready to apply

## Database Schema

### Tables Created

1. **ClientOrganisations**
   - Primary key: `Id` (uniqueidentifier)
   - Fields: Name, PrimaryContactId, Address, Timezone, IsActive, CreatedAt, UpdatedAt
   - Foreign key: PrimaryContactId → Users.Id

2. **Users**
   - Primary key: `Id` (uniqueidentifier)
   - Fields: Email, PasswordHash, DisplayName, Role (int enum), IsActive, ClientOrganisationId, CreatedAt, UpdatedAt
   - Unique index: Email
   - Foreign key: ClientOrganisationId → ClientOrganisations.Id
   - **Enum:** UserRole (Admin=0, Staff=1, Client=2)

3. **Requests**
   - Primary key: `Id` (uniqueidentifier)
   - Fields: ClientOrganisationId, CreatedByUserId, Title, Description, Status (int enum), Priority (int enum), DueDate, CreatedAt, UpdatedAt
   - Foreign keys:
     - ClientOrganisationId → ClientOrganisations.Id
     - CreatedByUserId → Users.Id
   - Indexes: ClientOrganisationId, Status, CreatedAt, CreatedByUserId
   - **Enums:**
     - RequestStatus: New=0, InProgress=1, WaitingOnClient=2, Resolved=3, Closed=4
     - RequestPriority: Low=0, Medium=1, High=2, Critical=3

4. **RequestComments**
   - Primary key: `Id` (uniqueidentifier)
   - Fields: RequestId, AuthorUserId, Message, IsInternal, CreatedAt
   - Foreign keys:
     - RequestId → Requests.Id (CASCADE delete)
     - AuthorUserId → Users.Id
   - Indexes: RequestId, AuthorUserId, CreatedAt

5. **Documents**
   - Primary key: `Id` (uniqueidentifier)
   - Fields: ClientOrganisationId, UploadedByUserId, FileName, BlobStoragePath, ContentType, FileSizeBytes, Category (int enum), VersionNumber, UploadedAt
   - Foreign keys:
     - ClientOrganisationId → ClientOrganisations.Id
     - UploadedByUserId → Users.Id
   - Indexes: ClientOrganisationId, UploadedAt
   - **Enum:** DocumentCategory (Contract=0, Report=1, Invoice=2, General=3)

6. **Notifications**
   - Primary key: `Id` (uniqueidentifier)
   - Fields: UserId, Type (int enum), Message, IsRead, RelatedRequestId, RelatedDocumentId, CreatedAt
   - Foreign key: UserId → Users.Id (CASCADE delete)
   - Indexes: UserId, CreatedAt, UserId+IsRead (composite)
   - **Enum:** NotificationType (RequestCreated=0, RequestUpdated=1, DocumentUploaded=2, CommentAdded=3)

## Applying the Migration

### Prerequisites

1. SQL Server must be running and accessible
2. Connection string configured in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=HelixPortalDb_Dev;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

### Apply Migration

From the solution root:

```bash
cd src/HelixPortal.Api
dotnet ef database update --project ../HelixPortal.Infrastructure --startup-project .
```

Or using the full path:

```bash
dotnet ef database update -p src/HelixPortal.Infrastructure -s src/HelixPortal.Api
```

### What Happens

1. EF Core checks if database `HelixPortalDb_Dev` exists
2. If not, creates the database
3. Creates `__EFMigrationsHistory` table to track applied migrations
4. Executes the InitialCreate migration:
   - Creates all 6 tables in dependency order
   - Creates all foreign key constraints
   - Creates all indexes
   - Records migration in `__EFMigrationsHistory`

### SQL Script

A complete SQL script has been generated at: `src/HelixPortal.Api/migration.sql`

This script can be executed directly in SQL Server Management Studio if needed.

## Verification

After applying the migration, verify:

1. Database `HelixPortalDb_Dev` exists
2. All 6 tables are created:
   - ClientOrganisations
   - Users
   - Requests
   - RequestComments
   - Documents
   - Notifications
3. `__EFMigrationsHistory` table contains entry for `20251203022547_InitialCreate`

### SQL Query to Verify

```sql
USE HelixPortalDb_Dev;
GO

-- Check tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Check migration history
SELECT * FROM __EFMigrationsHistory;
```

## Next Steps

After migration is applied:

1. Database seeding will run automatically (if configured in Program.cs)
2. Admin user will be created: `admin@helixportal.com` / `Admin@123!`
3. Sample client organisation and user will be created in Development environment

## Troubleshooting

### Error: "Cannot open database"

- Ensure SQL Server is running
- Check connection string is correct
- Verify SQL Server allows Windows Authentication (if using Trusted_Connection=True)

### Error: "Database already exists"

- Migration will still apply if not already applied
- Check `__EFMigrationsHistory` table to see applied migrations

### Error: "Migration already applied"

- Migration is idempotent - safe to run multiple times
- If migration was partially applied, you may need to manually fix the database state

