USE [StockTest]
GO

/****** Object:  Table [dbo].[StockHistory]    Script Date: 2/10/2021 11:08:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[StockHistory](
	[PointOfSale] [NVARCHAR] (MAX) NOT NULL,
	[Product] [NVARCHAR] (MAX) NOT NULL,
	[Date] [DATE] NOT NULL,
	[Stock] [INT] NOT NULL
) ON [PRIMARY]
GO


