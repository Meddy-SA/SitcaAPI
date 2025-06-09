```sql
-- Script para insertar el nuevo rol ATP en AspNetRoles
DECLARE @RoleId NVARCHAR(450) = NEWID();
DECLARE @RoleName NVARCHAR(256) = 'ATP';
DECLARE @NormalizedName NVARCHAR(256) = 'ATP';
DECLARE @ConcurrencyStamp NVARCHAR(MAX) = NEWID();

-- Verificar si el rol ya existe antes de insertarlo
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = @NormalizedName)
BEGIN
    INSERT INTO [dbo].[AspNetRoles] 
    (
        [Id], 
        [Name], 
        [NormalizedName], 
        [ConcurrencyStamp]
    )
    VALUES 
    (
        @RoleId,
        @RoleName,
        @NormalizedName,
        @ConcurrencyStamp
    );
    
    PRINT 'Rol ATP creado exitosamente con ID: ' + @RoleId;
END
ELSE
BEGIN
    PRINT 'El rol ATP ya existe en la base de datos.';
END
```
