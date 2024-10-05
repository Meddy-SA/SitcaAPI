ALTER TABLE [AspNetUsers] ADD [VencimientoCarnet] datetime2 NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230515235643_vencimiento-carnet-user', N'3.1.10');

GO

ALTER TABLE [AspNetUsers] ADD [AvisoVencimientoCarnet] datetime2 NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230515235752_aviso-vencimiento-carnet-user', N'3.1.10');

GO

