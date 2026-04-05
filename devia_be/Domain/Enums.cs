namespace devia_be.Domain;

public enum PlanTier
{
    Starter = 1,
    Growth = 2,
    Scale = 3
}

public enum UserStatus
{
    Activo = 1,
    Prueba = 2,
    Inactivo = 3
}

public enum AppStatus
{
    Produccion = 1,
    Beta = 2,
    Mantenimiento = 3,
    Archivada = 4
}

public enum DeploymentStatus
{
    Exitoso = 1,
    Fallido = 2,
    EnCola = 3
}

public enum DeploymentEnvironment
{
    Produccion = 1,
    Staging = 2
}

public enum SubscriptionStatus
{
    Activa = 1,
    Trial = 2,
    Cancelada = 3
}

public enum AuthRole
{
    User = 1,
    Admin = 2
}
