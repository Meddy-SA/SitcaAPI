ALTER TABLE [Notificacion] ADD [TextoInternoEn] nvarchar(200) NULL;

GO

ALTER TABLE [Notificacion] ADD [TextoParaEmpresaEn] nvarchar(200) NULL;

GO

ALTER TABLE [Notificacion] ADD [TituloInternoEn] nvarchar(150) NULL;

GO

ALTER TABLE [Notificacion] ADD [TituloParaEmpresaEn] nvarchar(150) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220331234258_campos-ingles-notificaciones', N'3.1.10');

GO

