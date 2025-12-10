# Proyecto E-commerce: Plataforma de Venta Minorista

Este repositorio contiene el código fuente de una plataforma de comercio electrónico desarrollada con **ASP.NET Core 7/8 (MVC)**, diseñada con un enfoque en la arquitectura limpia, patrones de diseño robustos y el uso de tecnologías modernas para la gestión de productos, pedidos y pagos.

## 1. Arquitectura y Patrones de Diseño

El proyecto está estructurado en un **diseño de capas (Layered Architecture)**, utilizando el patrón **Model-View-Controller (MVC)** para la presentación y el patrón **Repository y Unit of Work (UoW)** para el acceso a datos.

### 1.1. Inversión de Control (IoC) y Unit of Work

Se implementa la inyección de dependencias para desacoplar el acceso a datos. La clase `UnitOfWork` centraliza todas las operaciones de la base de datos, garantizando la **atomicidad y consistencia** de las transacciones (ej., al crear un pedido que implica insertar múltiples detalles). 

**`Ecommerce.DataAccess/Implementation/UnitOfWork.cs`**

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    // Repositorios expuestos por la Unidad de Trabajo
    public ICategoryRepository Category { get; private set; }
    public IProductRepository Product { get; private set; }
    // ... otros repositorios

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Category = new CategoryRepository(context);
        // Inicialización de todas las implementaciones de repositorios...
    }

    public int Complete()
    {
        // Centraliza el guardado de todos los cambios pendientes en una única transacción.
        return _context.SaveChanges();
    }
}

1.2. Repositorio Genérico y Eager Loading
La base de la capa de acceso a datos es un Repositorio Genérico (GenericRepository<T>), que proporciona las implementaciones CRUD estándar.

Este repositorio incluye una lógica avanzada para la carga anticipada (Eager Loading) de relaciones, permitiendo a los controladores especificar qué objetos relacionados deben incluirse en la consulta a través de un parámetro string (separado por comas).

Ecommerce.DataAccess/Implementation/GenericRepository.cs
public IEnumerable<T> GetAll(Expression<Func<T, bool>>? perdicate = null, string? Includeword = null)
{
    IQueryable<T> query = _dbSet;
    if (perdicate !=null) { query = query.Where(perdicate); }

    if (Includeword !=null)
    {
        // Implementación dinámica de Include() para múltiples relaciones
        foreach (var item in Includeword.Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(item); // Carga de datos optimizada
        }
    }
    return query.ToList();
}

2. Características Clave del Sistema
A. Gestión Administrativa (Areas/Admin)
El panel de administración está protegido con roles ([Authorize(Roles = SD.AdminRole)]) y centraliza las funcionalidades de gestión.

CRUD de Contenido: Implementación completa de Crear, Leer, Actualizar y Eliminar (CRUD) para Categorías y Productos.

Gestión de Imágenes: El ProductController utiliza IWebHostEnvironment para manejar la persistencia de archivos, incluyendo la lógica para eliminar la imagen antigua del servidor al subir un reemplazo.

Flujo de Pedidos: El OrderController gestiona la actualización de estados de pedidos (SD.Approve, SD.Proccessing, SD.Shipped) y registra los datos de seguimiento y envío.

Gestión de Usuarios: El UsersController permite al administrador bloquear/desbloquear usuarios mediante la propiedad LockoutEnd de ASP.NET Core Identity.

B. Flujo de Carrito y Pagos (Areas/Customer)
El CartController maneja la lógica central del checkout, la creación de pedidos y la integración de pagos.

Creación de Pedidos: Al confirmar la compra (POSTSummary), se realiza una secuencia atómica:

Se crea el OrderHeader.

Se crean los múltiples OrderDetail vinculados a los artículos del carrito.

Se llama a UoW.Complete().

Integración de Pasarela de Pagos (Stripe): El controlador está integrado con Stripe Checkout. Genera una SessionCreateOptions y utiliza SessionService para redirigir al usuario al portal de pagos de Stripe.

Ecommerce.Web/Areas/Customer/Controllers/CartController.cs
// Generación de la sesión de Stripe
var domain = "https://localhost:7056/";
var options = new SessionCreateOptions
{
    Mode = "payment",
    SuccessUrl = domain + $"customer/cart/orderconfirmation?id={ShoppingCartVM.OrderHeader.Id}",
    // ... configuración de artículos
};
var service = new SessionService();
Session session = service.Create(options);

Response.Headers.Add("Location", session.Url);
return new StatusCodeResult(303); // Redirección HTTP 303 a Stripe

onfirmación y Limpieza: El método OrderConfirmation verifica el estado de pago de la sesión de Stripe y, si es exitoso, actualiza el estado del pedido a SD.Approve y vacía el carrito del usuario (_unitOfWork.ShoppingCart.RemoveRange).

3. Prácticas de Ingeniería de Software
Práctica,Implementación
Separación de Intereses,"Uso de ViewModels (ProductVM, OrderVM) para evitar exponer los modelos de dominio directamente a las vistas."
Atomicidad de Datos,Patrón UoW implementado para transacciones de múltiples entidades (ej. Pedidos y Detalles).
Validación,"Uso de Tag Helpers (asp-validation-for) y _ValidationScriptsPartial para validación del lado del cliente, complementando la validación del lado del servidor (ModelState.IsValid)."
Mensajes Asíncronos,"Uso del patrón Post-Redirect-Get (PRG) y TempData con Toaster JS para mostrar notificaciones de éxito (TempData[""Create""], TempData[""Update""]) después de operaciones de POST y redirecciones."
Autenticación,"Implementación de ASP.NET Core Identity para el manejo de usuarios, roles y autenticación."

4. Tecnologías Principales
Backend: ASP.NET Core (MVC)

ORM: Entity Framework Core

Base de Datos: Configurada para SQL Server (implícita por EF Core y ApplicationDbContext).

Integración de Pagos: Stripe

Frontend (UI/UX): Bootstrap (a través de plantillas de administración como AdminLTE) y jQuery DataTables.
