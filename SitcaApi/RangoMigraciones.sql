BEGIN TRANSACTION;
GO

ALTER TABLE [ProcesoCertificacion] ADD [CreatedAt] datetime2 NULL;
GO

ALTER TABLE [ProcesoCertificacion] ADD [CreatedBy] nvarchar(max) NULL;
GO

ALTER TABLE [ProcesoCertificacion] ADD [Enabled] bit NULL;
GO

ALTER TABLE [ProcesoCertificacion] ADD [UpdatedAt] datetime2 NULL;
GO

ALTER TABLE [ProcesoCertificacion] ADD [UpdatedBy] nvarchar(max) NULL;
GO

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Pais_PaisId] FOREIGN KEY ([PaisId]) REFERENCES [Pais] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250226214650_AbstractEntity', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ProcesoCertificacion] DROP CONSTRAINT [FK_ProcesoCertificacion_Empresa_EmpresaId];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProcesoCertificacion]') AND [c].[name] = N'UpdatedBy');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ProcesoCertificacion] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [ProcesoCertificacion] ALTER COLUMN [UpdatedBy] nvarchar(450) NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProcesoCertificacion]') AND [c].[name] = N'Recertificacion');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ProcesoCertificacion] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [ProcesoCertificacion] ADD DEFAULT CAST(0 AS bit) FOR [Recertificacion];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProcesoCertificacion]') AND [c].[name] = N'Enabled');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [ProcesoCertificacion] DROP CONSTRAINT [' + @var2 + '];');
UPDATE [ProcesoCertificacion] SET [Enabled] = CAST(1 AS bit) WHERE [Enabled] IS NULL;
ALTER TABLE [ProcesoCertificacion] ALTER COLUMN [Enabled] bit NOT NULL;
ALTER TABLE [ProcesoCertificacion] ADD DEFAULT CAST(1 AS bit) FOR [Enabled];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProcesoCertificacion]') AND [c].[name] = N'CreatedBy');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ProcesoCertificacion] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [ProcesoCertificacion] ALTER COLUMN [CreatedBy] nvarchar(450) NULL;
GO

ALTER TABLE [ProcesoCertificacion] ADD [Cantidad] int NULL DEFAULT 0;
GO

CREATE TABLE [ProcesoArchivos] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Ruta] nvarchar(50) NOT NULL,
    [Tipo] nvarchar(10) NOT NULL,
    [FileTypesCompany] int NULL DEFAULT 0,
    [ProcesoCertificacionId] int NOT NULL,
    [Enabled] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedBy] nvarchar(450) NULL,
    [CreatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(450) NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ProcesoArchivos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProcesoArchivos_AspNetUsers_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProcesoArchivos_ProcesoCertificacion_ProcesoCertificacionId] FOREIGN KEY ([ProcesoCertificacionId]) REFERENCES [ProcesoCertificacion] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ProcesoCertificacion_CreatedBy] ON [ProcesoCertificacion] ([CreatedBy]);
GO

CREATE INDEX [IX_ProcesoArchivos_CreatedBy] ON [ProcesoArchivos] ([CreatedBy]);
GO

CREATE INDEX [IX_ProcesoArchivos_ProcesoCertificacionId] ON [ProcesoArchivos] ([ProcesoCertificacionId]);
GO

ALTER TABLE [ProcesoCertificacion] ADD CONSTRAINT [FK_ProcesoCertificacion_AspNetUsers_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [ProcesoCertificacion] ADD CONSTRAINT [FK_ProcesoCertificacion_Empresa_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [Empresa] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250227205030_ProcessUpdate', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProcesoArchivos]') AND [c].[name] = N'Ruta');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ProcesoArchivos] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [ProcesoArchivos] ALTER COLUMN [Ruta] nvarchar(150) NOT NULL;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Archivo]') AND [c].[name] = N'Ruta');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Archivo] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Archivo] ALTER COLUMN [Ruta] nvarchar(150) NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250305043530_ChangeSizeFiles', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ProcesoArchivos] ADD [FileSize] bigint NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250305235417_AddFileSize', N'8.0.8');
GO

COMMIT;
GO

