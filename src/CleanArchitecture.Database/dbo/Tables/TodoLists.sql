CREATE TABLE [dbo].[TodoLists] (
    [Id]             INT                IDENTITY (1, 1) NOT NULL,
    [Title]          NVARCHAR (200)     NOT NULL,
    [Colour_Code]    NVARCHAR (MAX)     NOT NULL,
    [Created]        DATETIMEOFFSET (7) NOT NULL,
    [CreatedBy]      NVARCHAR (MAX)     NULL,
    [LastModified]   DATETIMEOFFSET (7) NOT NULL,
    [LastModifiedBy] NVARCHAR (MAX)     NULL,
    CONSTRAINT [PK_TodoLists] PRIMARY KEY CLUSTERED ([Id] ASC)
);

