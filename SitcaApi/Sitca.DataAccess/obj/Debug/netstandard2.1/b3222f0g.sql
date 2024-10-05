CREATE TABLE [BackupCuestionario] (
    [Id] int NOT NULL IDENTITY,
    [CuestionarioId] int NOT NULL,
    [CuestionarioCompleto] nvarchar(max) NULL,
    CONSTRAINT [PK_BackupCuestionario] PRIMARY KEY ([Id])
);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230923210427_backup-cuestionario', N'3.1.10');

GO

