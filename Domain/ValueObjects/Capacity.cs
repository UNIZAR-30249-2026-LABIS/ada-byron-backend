namespace AdaByron.Domain.ValueObjects;

// TODO: Capacity — ValueObject inmutable que representa el aforo de un espacio
// Propiedades: MaxCapacity (int), CurrentOccupancyPercent (double)
// Método: EffectiveCapacity() → MaxCapacity * (1 - CurrentOccupancyPercent / 100)
// Equality estructural (no por referencia)
