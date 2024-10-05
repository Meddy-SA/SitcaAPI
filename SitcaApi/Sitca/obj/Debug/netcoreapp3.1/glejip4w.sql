CREATE TABLE [ActivityLog] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [User] nvarchar(max) NULL,
    [Observaciones] nvarchar(300) NULL,
    CONSTRAINT [PK_ActivityLog] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [CuestionarioItemObservaciones] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [Observaciones] nvarchar(1000) NULL,
    [CuestionarioItemId] int NOT NULL,
    [UsuarioCargaId] nvarchar(max) NULL,
    CONSTRAINT [PK_CuestionarioItemObservaciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CuestionarioItemObservaciones_CuestionarioItem_CuestionarioItemId] FOREIGN KEY ([CuestionarioItemId]) REFERENCES [CuestionarioItem] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_CuestionarioItemObservaciones_CuestionarioItemId] ON [CuestionarioItemObservaciones] ([CuestionarioItemId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220924162816_observaciones-preguntas-y-activitylog', N'3.1.10');

GO

