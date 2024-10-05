ALTER TABLE [Modulo] ADD [EnglishName] nvarchar(100) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220428000832_modulo_campo_ingles', N'3.1.10');

GO

