SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Empresa](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](200) NOT NULL,
	[Calle] [nvarchar](150) NOT NULL,
	[Numero] [nvarchar](60) NOT NULL,
	[Direccion] [nvarchar](150) NOT NULL,
	[Longitud] [nvarchar](20) NOT NULL,
	[Latitud] [nvarchar](20) NOT NULL,
	[IdPais] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[PaisId] [int] NULL,
	[IdNacional] [nvarchar](15) NOT NULL,
	[NombreRepresentante] [nvarchar](150) NOT NULL,
	[Telefono] [nvarchar](15) NOT NULL,
	[CargoRepresentante] [nvarchar](60) NOT NULL,
	[Ciudad] [nvarchar](50) NOT NULL,
	[Estado] [decimal](18, 2) NULL,
	[Email] [nvarchar](50) NOT NULL,
	[WebSite] [nvarchar](100) NOT NULL,
	[ResultadoActual] [nvarchar](max) NOT NULL,
	[ResultadoSugerido] [nvarchar](max) NOT NULL,
	[ResultadoVencimiento] [datetime2](7) NULL,
	[FechaAutoNotif] [datetime2](7) NULL,
	[EsHomologacion] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [PK_Empresa] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Empresa_PaisId] ON [dbo].[Empresa]
(
	[PaisId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_Nombre]  DEFAULT (N'') FOR [Nombre]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_Calle]  DEFAULT (N'') FOR [Calle]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_Numero]  DEFAULT (N'') FOR [Numero]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_Direccion]  DEFAULT (N'') FOR [Direccion]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_Longitud]  DEFAULT (N'') FOR [Longitud]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_Latitud]  DEFAULT (N'') FOR [Latitud]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_IdNacional]  DEFAULT (N'') FOR [IdNacional]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_NombreRepresentante]  DEFAULT (N'') FOR [NombreRepresentante]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_Telefono]  DEFAULT (N'') FOR [Telefono]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_CargoRepresentante]  DEFAULT (N'') FOR [CargoRepresentante]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_Ciudad]  DEFAULT (N'') FOR [Ciudad]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_Email]  DEFAULT (N'') FOR [Email]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_WebSite]  DEFAULT (N'') FOR [WebSite]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_ResultadoActual]  DEFAULT (N'') FOR [ResultadoActual]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_ResultadoSugerido]  DEFAULT (N'') FOR [ResultadoSugerido]
GO
ALTER TABLE [dbo].[Empresa] ADD  DEFAULT (CONVERT([bit],(0))) FOR [EsHomologacion]
GO
ALTER TABLE [dbo].[Empresa]  WITH CHECK ADD  CONSTRAINT [FK_Empresa_Pais_PaisId] FOREIGN KEY([PaisId])
REFERENCES [dbo].[Pais] ([Id])
GO
ALTER TABLE [dbo].[Empresa] CHECK CONSTRAINT [FK_Empresa_Pais_PaisId]
GO
