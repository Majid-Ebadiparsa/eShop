/****** Object:  Table [dbo].[Test_Table]    Script Date: 6/23/2025 8:41:10 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Test_Table]') AND type in (N'U'))
DROP TABLE [dbo].[Test_Table]
GO

/****** Object:  Table [dbo].[Test_Table]    Script Date: 6/23/2025 8:41:10 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Test_Table](
	[xserver_name] [varchar](30) NOT NULL,
	[xdttm_ins] [datetime] NOT NULL,
	[xdttm_last_ins_upd] [datetime] NOT NULL,
	[xfallback_dbid] [smallint] NULL,
	[name] [varchar](30) NOT NULL,
	[dbid] [smallint] NOT NULL,
	[status] [smallint] NOT NULL,
	[version] [smallint] NOT NULL
) ON [PRIMARY]
GO


