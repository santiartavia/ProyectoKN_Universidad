"""Prepara el esquema Azure desde la definición revisada del proyecto."""
from pathlib import Path
import re
import hashlib
import base64
import secrets

ROOT = Path(__file__).resolve().parents[2]
out = ROOT / 'BD' / 'Azure'
out.mkdir(parents=True, exist_ok=True)
source = (ROOT / 'INSTALAR_BASE_DATOS_COMPLETA.sql').read_text(encoding='utf-8-sig')
schema = source[source.index('-- Roles\n'):source.index('-- 3. CARGA DE DATOS INICIALES')]
# Ventas rápidas no tienen necesariamente un pedido asociado.
schema = schema.replace('[id_venta] INT IDENTITY(1,1) NOT NULL,\n    [id_pedido] INT NOT NULL,', '[id_venta] INT IDENTITY(1,1) NOT NULL,\n    [id_pedido] INT NULL,')
schema = schema.replace("DEFAULT 'activa'", "DEFAULT 'ACTIVA'").replace("DEFAULT 'en_mesa'", "DEFAULT 'mesa'")
schema = schema.replace('[dispositivo] NVARCHAR(250)', '[dispositivo] NVARCHAR(500)').replace('[dispositivo_acceso] NVARCHAR(250)', '[dispositivo_acceso] NVARCHAR(500)')
batches = [b.strip() for b in re.split(r'^GO\s*$', schema, flags=re.M) if b.strip() and 'CREATE TABLE' in b]
for name in ['Envios_PDV', 'Subcuentas_Venta', 'Subcuenta_Detalle_Venta']:
    table = (ROOT / 'BD' / 'dbo' / 'Tables' / (name + '.sql')).read_text(encoding='utf-8-sig')
    parts = [p.strip() for p in re.split(r'^GO\s*$', table, flags=re.M) if p.strip()]
    batches.append(f"IF OBJECT_ID(N'dbo.{name}', N'U') IS NULL\nBEGIN\n{parts[0]}\nEND")
    for part in parts[1:]:
        index = re.search(r'CREATE NONCLUSTERED INDEX \[([^]]+)\]', part)
        if index:
            batches.append(f"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.{name}') AND name=N'{index[1]}')\n{part}")
batches.append("""-- Compatibilidad no destructiva para instalaciones anteriores del mismo esquema.
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Ventas') AND name=N'id_pedido' AND is_nullable=0)
    ALTER TABLE dbo.Ventas ALTER COLUMN id_pedido INT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Detalle_Pedido') AND name=N'cantidad' AND system_type_id<>TYPE_ID(N'int'))
    THROW 51001, 'Detalle_Pedido.cantidad debe ser INT. Revise sus datos antes de migrar ese tipo.', 1;
""")
# Una sola instrucción por archivo: también funciona en editores sin separador GO.
header = """-- 01 - ESQUEMA COMPLETO PARA AZURE SQL DATABASE
-- Ejecutar conectado DIRECTAMENTE a la base de la aplicación, ya creada en Azure.
-- Sin USE, CREATE DATABASE, GO, SQLCMD, usuarios del servidor ni semillas.
-- Crea tablas faltantes. No borra registros ni reemplaza tablas existentes.
-- Compatible con el esquema INSTALAR_BASE_DATOS_COMPLETA.sql del proyecto.
-- No es una migración universal de bases con columnas o restricciones diferentes.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 51000, 'Seleccione la base de datos de la aplicación antes de ejecutar.', 1;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRY
    BEGIN TRANSACTION;
"""
body = '\n\n'.join("    EXEC sys.sp_executesql N'" + batch.replace("'", "''") + "';" for batch in batches)
footer = """
    COMMIT TRANSACTION;
    SELECT DB_NAME() AS base_de_datos, COUNT(*) AS tablas_dbo FROM sys.tables WHERE schema_id=SCHEMA_ID(N'dbo');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
"""
(out / '01_esquema_azure.sql').write_text(header + body + footer, encoding='utf-8-sig')
seed = (ROOT / 'tools' / 'qa' / 'azure_seed_template.sql').read_text(encoding='utf-8-sig')
for role in ['ADMIN', 'MESERO', 'COCINA', 'CAJA']:
    salt = secrets.token_bytes(16)
    key = hashlib.pbkdf2_hmac('sha256', b'ColibriDemo2026!', salt, 600000, 32)
    value = 'pbkdf2-sha256$600000$' + base64.b64encode(salt + key).decode('ascii')
    seed = seed.replace('__HASH_' + role + '__', value)
(out / '02_datos_demo_pequenos.sql').write_text(seed, encoding='utf-8-sig')
print(f'Esquema Azure: {len(batches)} bloques; {len(re.findall("CREATE TABLE", body))} tablas.')
