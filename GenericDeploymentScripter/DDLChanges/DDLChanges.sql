CREATE TABLE dbo.DDLChanges

(

    EventDate    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    EventType    NVARCHAR(100),

    EventDDL     NVARCHAR(MAX),

    DatabaseName NVARCHAR(255),

    SchemaName   NVARCHAR(255),

    ObjectName   NVARCHAR(255),

    HostName     NVARCHAR(255),

    IPAddress    VARCHAR(32),

    ProgramName  NVARCHAR(255),

    LoginName    NVARCHAR(255)

);

GO

INSERT dbo.DDLChanges

(

    EventType,

    EventDDL,

    DatabaseName,

    SchemaName,

    ObjectName

)

SELECT

    N'Initial control',

    OBJECT_DEFINITION([object_id]),

    DB_NAME(),

    OBJECT_SCHEMA_NAME([object_id]),

    OBJECT_NAME([object_id])

FROM

    sys.objects so

                WHERE so.[type] IN ('FN', 'P', 'TF', 'V');

GO

 

CREATE TRIGGER [CaptureDDLChanges]

    ON DATABASE

    FOR CREATE_PROCEDURE, ALTER_PROCEDURE, DROP_PROCEDURE, ALTER_VIEW, CREATE_VIEW, DROP_VIEW,

                               CREATE_TABLE, ALTER_TABLE, DROP_TABLE, CREATE_FUNCTION, ALTER_FUNCTION, DROP_FUNCTION

AS

BEGIN

    SET NOCOUNT ON;

 

    DECLARE @EventData XML = EVENTDATA(), @ip VARCHAR(32);

 

    SELECT @ip = client_net_address

        FROM sys.dm_exec_connections

        WHERE session_id = @@SPID;

 

    INSERT dbo.DDLChanges

    (

        EventType,

        EventDDL,

        SchemaName,

        ObjectName,

        DatabaseName,

        HostName,

        IPAddress,

        ProgramName,

        LoginName

    )

    SELECT

        @EventData.value('(/EVENT_INSTANCE/EventType)[1]',   'NVARCHAR(100)'),

        @EventData.value('(/EVENT_INSTANCE/TSQLCommand)[1]', 'NVARCHAR(MAX)'),

        @EventData.value('(/EVENT_INSTANCE/SchemaName)[1]',  'NVARCHAR(255)'),

        @EventData.value('(/EVENT_INSTANCE/ObjectName)[1]',  'NVARCHAR(255)'),

        DB_NAME(), HOST_NAME(), @ip, PROGRAM_NAME(),

                               CASE WHEN ORIGINAL_LOGIN() <> SUSER_SNAME() THEN ORIGINAL_LOGIN() + ' as ' + SUSER_NAME() ELSE SUSER_NAME() END;

END