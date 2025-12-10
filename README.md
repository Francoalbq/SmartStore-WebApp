# Proyecto E-commerce: Plataforma de Venta Minorista

Este repositorio contiene el código fuente de una plataforma de comercio electrónico desarrollada con ASP.NET Core 7/8 (MVC). El proyecto está diseñado con arquitectura limpia, buenas prácticas y patrones de diseño enfocados en la gestión integral de productos, pedidos y pagos.

## 1. Arquitectura y Patrones de Diseño

El proyecto utiliza una arquitectura por capas (Layered Architecture), con MVC para la presentación y los patrones Repository y Unit of Work (UoW) para el acceso a datos.

### 1.1 Inversión de Control (IoC) y Unit of Work

Se implementa inyección de dependencias para desacoplar la lógica de acceso a datos. La clase UnitOfWork centraliza todas las operaciones de base de datos.

Ruta: Ecommerce.DataAccess/Implementation/UnitOfWork.cs

Código (UnitOfWork.cs):
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    public ICategoryRepository Category { get; private set; }
    public IProductRepository Product { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Category = new CategoryRepository(context);
    }

    public int Complete()
    {
        return _context.SaveChanges();
    }
}

### 1.2 Repositorio Genérico y Eager Loading

Se utiliza un GenericRepository<T> para CRUD. Incluye soporte para carga anticipada dinámica con Include().

Ruta: Ecommerce.DataAccess/Implementation/GenericRepository.cs

Código (GenericRepository.cs):
public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null, string? includeWord = null)
{
    IQueryable<T> query = _dbSet;

    if (predicate != null)
        query = query.Where(predicate);

    if (includeWord != null)
    {
        foreach (var item in includeWord.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(item);
        }
    }

    return query.ToList();
}

## 2. Características del Sistema

### A. Gestión Administrativa (Areas/Admin)

Funciones:

- CRUD de Categorías y Productos
- Gestión de imágenes con IWebHostEnvironment
- Actualización de estados de pedidos (Approved, Processing, Shipped)
- Administración de usuarios mediante LockoutEnd en Identity

### B. Carrito y Pagos (Areas/Customer)

Incluye la lógica completa del checkout, creación de pedidos e integración con Stripe.

#### Flujo del Checkout

1. Creación de OrderHeader
2. Creación de OrderDetail
3. Ejecución de UnitOfWork.Complete()

#### Integración con Stripe

Código (Stripe):
var domain = "https://localhost:7056/";
var options = new SessionCreateOptions
{
    Mode = "payment",
    SuccessUrl = domain + $"customer/cart/orderconfirmation?id={ShoppingCartVM.OrderHeader.Id}"
};
var service = new SessionService();
Session session = service.Create(options);
Response.Headers.Add("Location", session.Url);
return new StatusCodeResult(303);

#### Confirmación de Pago

El método OrderConfirmation:

- Verifica el estado del pago
- Actualiza el pedido a Approved
- Elimina el contenido del carrito

## 3. Prácticas de Ingeniería de Software

- Uso de ViewModels (ProductVM, OrderVM)
- Patrón Unit of Work para atomicidad
- Validación con Tag Helpers y ModelState
- Patrón PRG + TempData para notificaciones
- Autenticación con ASP.NET Core Identity

## 4. Tecnologías Principales

Backend: ASP.NET Core MVC  
ORM: Entity Framework Core  
Base de Datos: SQL Server  
Pagos: Stripe  
Frontend: Bootstrap, jQuery, DataTables, AdminLTE

