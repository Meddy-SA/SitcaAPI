ALTER TABLE [AspNetUsers] ADD [Lenguage] nvarchar(3) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220222214515_user-language', N'3.1.10');

GO

ALTER TABLE [Tipologia] ADD [NameEnglish] nvarchar(75) NULL;

GO

ALTER TABLE [SubtituloSeccion] ADD [NameEnglish] nvarchar(100) NULL;

GO

ALTER TABLE [SeccionModulo] ADD [NameEnglish] nvarchar(100) NULL;

GO

ALTER TABLE [Pregunta] ADD [Text] nvarchar(2500) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220225000852_CamposBD-Ingles', N'3.1.10');

GO

ALTER TABLE [ResultadoCertificacion] ADD [NumeroDictamen] nvarchar(50) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220304194807_calificacion-dictamen', N'3.1.10');

GO

