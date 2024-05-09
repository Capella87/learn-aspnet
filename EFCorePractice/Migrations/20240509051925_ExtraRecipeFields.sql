IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507172916_InitialSchema'
)
BEGIN
    CREATE TABLE [Recipes] (
        [RecipeId] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [TimeToCook] time NOT NULL,
        [IsDeleted] bit NOT NULL,
        [Method] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Recipes] PRIMARY KEY ([RecipeId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507172916_InitialSchema'
)
BEGIN
    CREATE TABLE [Ingredient] (
        [IngredientId] int NOT NULL IDENTITY,
        [RecipeId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Ingredient] PRIMARY KEY ([IngredientId]),
        CONSTRAINT [FK_Ingredient_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([RecipeId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507172916_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Ingredient_RecipeId] ON [Ingredient] ([RecipeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507172916_InitialSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240507172916_InitialSchema', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240509051925_ExtraRecipeFields'
)
BEGIN
    ALTER TABLE [Recipes] ADD [IsVegan] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240509051925_ExtraRecipeFields'
)
BEGIN
    ALTER TABLE [Recipes] ADD [IsVegetarian] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240509051925_ExtraRecipeFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240509051925_ExtraRecipeFields', N'8.0.4');
END;
GO

COMMIT;
GO

