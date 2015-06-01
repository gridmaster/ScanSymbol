USE [Markets]
GO

/****** Object:  Table [dbo].[Sectors]    Script Date: 05/31/2015 18:21:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Sectors](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [date] NULL,
	[Name] [nvarchar](60) NULL,
	[URI] [nvarchar] (300) NULL,
	[OneDayPriceChangePercent] [decimal](18, 3) NULL,
	[MarketCap] [nvarchar](30) NULL,
	[PriceToEarnings] [decimal](18, 3) NULL,
	[ROEPercent] [decimal](18, 3) NULL,
	[DivYieldPercent] [decimal](18, 3) NULL,
	[LongTermDebtToEquity] [decimal](18, 3) NULL,
	[PriceToBookValue] [decimal](18, 3) NULL,
	[NetProfitMarginPercentMRQ] [decimal](18, 3) NULL,
	[PriceToFreeCashFlowMRQ] [decimal](18, 3) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


