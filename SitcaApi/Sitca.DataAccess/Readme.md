# Tareas a realizar!

Para publicar el 30/1/2025.

 - [ ] Crear Indice de AspNetUsers
```SQL
CREATE  NONCLUSTERED  INDEX IX_AspNetUsers_PaisId_Active_Notificaciones ON AspNetUsers(PaisId, Active, Notificaciones) INCLUDE (Email, FirstName, LastName, Lenguage);
```

- [ ] Crear migración de relacion de AspNetUsers con País.
```BASH
dotnet ef migrations add AddPaisUserRelationship
```
```
```
- [ ] Usuario Asesor b19238db-bfa1-4707-ba63-bbd8006c3a00 no encontrado en la lista
- [ ] Mejorar Informes Avanzados.

