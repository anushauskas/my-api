CREATE TABLE [dbo].[TodoItems] (
    [Id]             INT                IDENTITY (1, 1) NOT NULL,
    [ListId]         INT                NOT NULL,
    [Title]          NVARCHAR (200)     NOT NULL,
    [Note]           NVARCHAR (MAX)     NULL,
    [Priority]       INT                NOT NULL,
    [Reminder]       DATETIME2 (7)      NULL,
    [Done]           BIT                NOT NULL,
    [Created]        DATETIMEOFFSET (7) NOT NULL,
    [CreatedBy]      NVARCHAR (MAX)     NULL,
    [LastModified]   DATETIMEOFFSET (7) NOT NULL,
    [LastModifiedBy] NVARCHAR (MAX)     NULL,
    CONSTRAINT [PK_TodoItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TodoItems_TodoLists_ListId] FOREIGN KEY ([ListId]) REFERENCES [dbo].[TodoLists] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_TodoItems_ListId]
    ON [dbo].[TodoItems]([ListId] ASC);

