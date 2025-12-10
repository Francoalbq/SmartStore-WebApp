using Ecommerce.Entities.Models;
using Ecommerce.Entities.Repositories;
using Ecommerce.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X.PagedList;

namespace Ecommerce.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitofwork;

        public HomeController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }
        public IActionResult Index(int? page)
        {
            var PageNumber = page ?? 1;
            int PageSize = 8;


            var products = _unitofwork.Product.GetAll().ToPagedList(PageNumber, PageSize);
            return View(products);
        }
        [HttpGet]
        public IActionResult Details(int ProductId)
        {
            ShoppingCart obj = new ShoppingCart()
            {
                ProductId = ProductId,
                Product = _unitofwork.Product.GetFirstorDefault(v => v.Id == ProductId, Includeword: "Category"),
                Count = 1
            };
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart Cartobj = _unitofwork.ShoppingCart.GetFirstorDefault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

            if (Cartobj == null)
            {
                _unitofwork.ShoppingCart.Add(shoppingCart);
                _unitofwork.Complete();
                HttpContext.Session.SetInt32(SD.SessionKey,
                    _unitofwork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count()
                   );

            }
            else
            {
                _unitofwork.ShoppingCart.IncreaseCount(Cartobj, shoppingCart.Count);
                _unitofwork.Complete();
            }


            return RedirectToAction("Index");
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult FAQs()
        {
            return View();
        }
    }
}
