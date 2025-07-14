
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cuestionario](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpresa] [int] NOT NULL,
	[FechaInicio] [datetime2](7) NOT NULL,
	[FechaGenerado] [datetime2](7) NOT NULL,
	[FechaFinalizado] [datetime2](7) NULL,
	[TipologiaId] [int] NULL,
	[IdTipologia] [int] NOT NULL,
	[Resultado] [int] NOT NULL,
	[Prueba] [bit] NOT NULL,
	[AsesorId] [nvarchar](max) NULL,
	[AuditorId] [nvarchar](max) NULL,
	[FechaVisita] [datetime2](7) NULL,
	[ProcesoCertificacionId] [int] NULL,
	[FechaRevisionAuditor] [datetime2](7) NULL,
	[TecnicoPaisId] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Cuestionario] ADD  CONSTRAINT [PK_Cuestionario] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Cuestionario_ProcesoCertificacionId] ON [dbo].[Cuestionario]
(
	[ProcesoCertificacionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Cuestionario_TipologiaId] ON [dbo].[Cuestionario]
(
	[TipologiaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Cuestionario] ADD  DEFAULT (CONVERT([bit],(0))) FOR [Prueba]
GO
ALTER TABLE [dbo].[Cuestionario]  WITH CHECK ADD  CONSTRAINT [FK_Cuestionario_ProcesoCertificacion_ProcesoCertificacionId] FOREIGN KEY([ProcesoCertificacionId])
REFERENCES [dbo].[ProcesoCertificacion] ([Id])
GO
ALTER TABLE [dbo].[Cuestionario] CHECK CONSTRAINT [FK_Cuestionario_ProcesoCertificacion_ProcesoCertificacionId]
GO
ALTER TABLE [dbo].[Cuestionario]  WITH CHECK ADD  CONSTRAINT [FK_Cuestionario_Tipologia_TipologiaId] FOREIGN KEY([TipologiaId])
REFERENCES [dbo].[Tipologia] ([Id])
GO
ALTER TABLE [dbo].[Cuestionario] CHECK CONSTRAINT [FK_Cuestionario_Tipologia_TipologiaId]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CuestionarioItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Texto] [nvarchar](2500) NOT NULL,
	[Nomenclatura] [nvarchar](30) NOT NULL,
	[ResultadoAuditor] [bit] NOT NULL,
	[Obligatorio] [bit] NOT NULL,
	[CuestionarioId] [int] NOT NULL,
	[PreguntaId] [int] NOT NULL,
	[Resultado] [int] NOT NULL,
	[FechaActualizado] [datetime2](7) NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CuestionarioItem] ADD  CONSTRAINT [PK_CuestionarioItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_CuestionarioItem_CuestionarioId] ON [dbo].[CuestionarioItem]
(
	[CuestionarioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_CuestionarioItem_PreguntaId] ON [dbo].[CuestionarioItem]
(
	[PreguntaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CuestionarioItem] ADD  CONSTRAINT [DF_CuestionarioItem_Texto]  DEFAULT ('') FOR [Texto]
GO
ALTER TABLE [dbo].[CuestionarioItem] ADD  CONSTRAINT [DF_CuestionarioItem_Nomenclatura]  DEFAULT ('') FOR [Nomenclatura]
GO
ALTER TABLE [dbo].[CuestionarioItem] ADD  CONSTRAINT [DF__Cuestiona__Resul__1F2E9E6D]  DEFAULT ((0)) FOR [Resultado]
GO
ALTER TABLE [dbo].[CuestionarioItem]  WITH CHECK ADD  CONSTRAINT [FK_CuestionarioItem_Cuestionario_CuestionarioId] FOREIGN KEY([CuestionarioId])
REFERENCES [dbo].[Cuestionario] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CuestionarioItem] CHECK CONSTRAINT [FK_CuestionarioItem_Cuestionario_CuestionarioId]
GO
ALTER TABLE [dbo].[CuestionarioItem]  WITH CHECK ADD  CONSTRAINT [FK_CuestionarioItem_Pregunta_PreguntaId] FOREIGN KEY([PreguntaId])
REFERENCES [dbo].[Pregunta] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CuestionarioItem] CHECK CONSTRAINT [FK_CuestionarioItem_Pregunta_PreguntaId]
GO

