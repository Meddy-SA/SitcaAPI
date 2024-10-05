ALTER TABLE [ProcesoCertificacion] ADD [Recertificacion] bit NOT NULL DEFAULT CAST(0 AS bit);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220521140646_recertificacion-field-procesocertificacion', N'3.1.10');

GO

ALTER TABLE [Empresa] ADD [EsHomologacion] bit NOT NULL DEFAULT CAST(0 AS bit);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220602222700_homologacion', N'3.1.10');

GO

CREATE TABLE [Homologacion] (
    [Id] int NOT NULL IDENTITY,
    [Distintivo] nvarchar(70) NULL,
    [DatosProceso] nvarchar(1000) NULL,
    [FechaOtorgamiento] datetime2 NOT NULL,
    [FechaVencimiento] datetime2 NOT NULL,
    [EmpresaId] int NOT NULL,
    [CertificacionId] int NOT NULL,
    CONSTRAINT [PK_Homologacion] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Homologacion_ProcesoCertificacion_CertificacionId] FOREIGN KEY ([CertificacionId]) REFERENCES [ProcesoCertificacion] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Homologacion_Empresa_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [Empresa] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_Homologacion_CertificacionId] ON [Homologacion] ([CertificacionId]);

GO

CREATE INDEX [IX_Homologacion_EmpresaId] ON [Homologacion] ([EmpresaId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220602222838_homologacion2', N'3.1.10');

GO

ALTER TABLE [Distintivo] ADD [NameEnglish] nvarchar(40) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220604225101_distintivo-language', N'3.1.10');

GO

