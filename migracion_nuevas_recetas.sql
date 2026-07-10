-- Migración completa: nuevo esquema.

-- Borrar tabla original de recetas si existe (para evitar conflictos)
DROP TABLE IF EXISTS dbo.Recetas;

-- 1. Crear tabla header Receta
IF OBJECT_ID('dbo.Receta', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Receta (
        id_receta   INT IDENTITY(1,1) NOT NULL,
        id_producto INT NOT NULL,
        estado      BIT NOT NULL CONSTRAINT DF_Receta_estado DEFAULT 1,
        CONSTRAINT PK_Receta PRIMARY KEY (id_receta),
        CONSTRAINT UQ_Receta_Producto UNIQUE (id_producto),
        CONSTRAINT FK_Receta_Producto FOREIGN KEY (id_producto) REFERENCES dbo.Productos(id_producto)
    );
END
GO

-- 2. Crear tabla detail RecetaInsumo
IF OBJECT_ID('dbo.RecetaInsumo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RecetaInsumo (
        id_receta     INT            NOT NULL,
        id_insumo     INT            NOT NULL,
        cantidad_usar DECIMAL(10,4)  NOT NULL CONSTRAINT CK_RI_cant CHECK (cantidad_usar > 0),
        CONSTRAINT PK_RecetaInsumo PRIMARY KEY (id_receta, id_insumo),
        CONSTRAINT FK_RI_Receta FOREIGN KEY (id_receta) REFERENCES dbo.Receta(id_receta),
        CONSTRAINT FK_RI_Insumo FOREIGN KEY (id_insumo) REFERENCES dbo.Insumos(id_insumo)
    );
END
GO


-- 3. Asegurar insumos (solo si no existen los de receta)
IF NOT EXISTS (SELECT 1 FROM dbo.Insumos WHERE id_insumo = 2)
BEGIN
    SET IDENTITY_INSERT dbo.Insumos ON;
    INSERT INTO dbo.Insumos (id_insumo, id_categoria, nombre_insumo, unidad_medida, stock_minimo, stock_actual, costo_unitario, estado) VALUES
        (2,  2, 'Carne molida',       'Kg',      10, 25, 4500, 1),
        (3,  2, 'Pollo entero',       'Kg',       8, 20, 3200, 1),
        (4,  2, 'Pescado fresco',     'Kg',       6, 12, 5500, 1),
        (5,  2, 'Camarones',          'Kg',       5, 10, 8000, 1),
        (6,  4, 'Papa',               'Kg',      15, 40,  800, 1),
        (7,  4, 'Vegetales mixtos',   'Kg',      10, 18, 1200, 1),
        (8,  4, 'Lechuga',           'Unidad',  10, 30,  500, 1),
        (9,  4, 'Plátano',           'Unidad',  10, 25,  350, 1),
        (10, 5, 'Arroz',             'Kg',      20, 50, 1100, 1),
        (11, 5, 'Frijoles',          'Kg',      15, 30, 1500, 1),
        (12, 5, 'Pasta',             'Kg',      10, 22, 1800, 1),
        (13, 5, 'Pan de hamburguesa','Unidad',  20, 40,  600, 1),
        (14, 3, 'Leche',             'L',       15, 35,  950, 1),
        (15, 3, 'Crema dulce',       'L',        8, 15, 2200, 1),
        (16, 3, 'Queso parmesano',   'Kg',       5, 10, 6500, 1),
        (17, 3, 'Mantequilla',       'Kg',       5, 12, 2800, 1),
        (18, 6, 'Condimentos Varios', 'Kg',      5,  8, 3000, 1);
    SET IDENTITY_INSERT dbo.Insumos OFF;
END
GO

-- 5. Poblar recetas (solo si no hay datos)
IF NOT EXISTS (SELECT 1 FROM dbo.Receta)
BEGIN
    -- 1 = Carne con puré
    INSERT INTO dbo.Receta (id_producto, estado) VALUES (1, 1);
    INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES
        (1, 2, 0.2000), (1, 6, 0.3000), (1, 14, 0.1000), (1, 17, 0.0500);

    -- 2 = Pescado con vegetales
    INSERT INTO dbo.Receta (id_producto, estado) VALUES (2, 1);
    INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES
        (2, 4, 0.2500), (2, 7, 0.2000);

    -- 3 = Casado de pollo
    INSERT INTO dbo.Receta (id_producto, estado) VALUES (3, 1);
    INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES
        (3, 3, 0.2500), (3, 10, 0.2000), (3, 11, 0.1500), (3, 9, 1.0000);

    -- 4 = Hamburguesa con papas
    INSERT INTO dbo.Receta (id_producto, estado) VALUES (4, 1);
    INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES
        (4, 2, 0.1800), (4, 13, 1.0000), (4, 6, 0.2000), (4, 8, 2.0000);

    -- 5 = Pasta Alfredo
    INSERT INTO dbo.Receta (id_producto, estado) VALUES (5, 1);
    INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES
        (5, 12, 0.2500), (5, 15, 0.1500), (5, 16, 0.0500), (5, 17, 0.0300);

    -- 6 = Arroz con camarones
    INSERT INTO dbo.Receta (id_producto, estado) VALUES (6, 1);
    INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES
        (6, 10, 0.2500), (6, 5, 0.1500), (6, 7, 0.1000), (6, 18, 0.0200);
END
GO
