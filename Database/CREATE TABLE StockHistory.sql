USE [Stock]
GO

/****** Object:  Table [dbo].[StockHistory]    Script Date: 2/10/2021 11:08:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[StockHistory](
	[PointOfSale] [int] NOT NULL,
	[Product] [int] NOT NULL,
	[Date] [date] NOT NULL,
	[Stock] [int] NOT NULL
) ON [PRIMARY]
GO


