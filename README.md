# MLCodingChallenge

API backend que expone datos de productos y permite comparar múltiples items lado a lado, filtrando por los campos que interesen.

## Configuración

```bash
dotnet restore
dotnet run
```

La API inicia en `http://localhost:5000` por defecto.

## Endpoints

### GET /api/products

Devuelve el catálogo completo de productos.

**Respuesta:** array de objetos `Product`.

---

### GET /api/products/compare

Compara dos o más productos. Opcionalmente filtra por campos específicos.

| Parámetro | Requerido | Descripción |
|-----------|-----------|-------------|
| `ids`     | Sí        | IDs de productos separados por coma (mínimo 2) |
| `fields`  | No        | Nombres de campos separados por coma para incluir en la comparación |

**Campos disponibles:** `id`, `name`, `description`, `imageUrl`, `price`, `rating`, `category`, `size`, `weight`, `color`, `specifications`

**Ejemplos de solicitud:**

```
GET /api/products/compare?ids=1,2,3
GET /api/products/compare?ids=1,2&fields=name,price,rating,specifications
```

**Ejemplo de respuesta (filtrada):**

```json
{
  "products": [
    {
      "name": "Samsung Galaxy S24 Ultra",
      "price": 1299.99,
      "rating": 4.7
    },
    {
      "name": "iPhone 15 Pro Max",
      "price": 1199.99,
      "rating": 4.8
    }
  ],
  "fields": ["name", "price", "rating"]
}
```

## Arquitectura

```
Controllers/        -> Capa HTTP, validación, ruteo
Services/           -> Lógica de negocio (comparación, filtrado de campos)
Data/               -> Repositorio + persistencia en JSON
Models/             -> DTOs y entidades de dominio
```

- **Repository Pattern** para abstraer el origen de datos. Cambiar JSON por una BD solo requiere una nueva implementación de `IProductRepository`.
- **DI** configurada en `Program.cs`.
- Las especificaciones de producto se almacenan como `Dictionary<string, string>` para que cada categoría tenga sus propios atributos sin necesidad de tipos adicionales.

## Decisiones de diseño

- **Archivo JSON** como fuente de datos por simplicidad. El repositorio lo carga una vez al iniciar (singleton) evitando I/O repetido a disco.
- **Filtrado de campos** funciona serializando el producto a un diccionario en tiempo de ejecución y conservando solo las claves solicitadas. Esto evita hardcodear un switch por cada campo.
- **Validación** se maneja a nivel del controller: IDs inválidos, parámetros faltantes y validaciones de cantidad mínima devuelven mensajes de error claros.
- **Sin capas adicionales** (Domain, MediatR, CQRS, etc.): para el alcance de este challenge no tiene sentido. Controller → Service → Repository cubre lo necesario, y si mañana hay que escalar, las interfaces ya están para cambiar implementaciones sin romper nada.
