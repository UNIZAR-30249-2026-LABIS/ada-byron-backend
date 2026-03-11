namespace AdaByron.Domain.Enums;

// Tipo de espacio físico del edificio Ada Byron.
// Se usa en dos propiedades de Espacio:
//   TipoFisico       → inmutable (naturaleza constructiva del espacio)
//   CategoriaReserva → mutable según la Matriz de Mutabilidad (Regla C)
public enum TipoEspacio
{
    Aula,
    Laboratorio,
    Seminario,
    SalaComun,
    Despacho,
}
