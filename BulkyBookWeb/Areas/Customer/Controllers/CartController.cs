using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
		public CartController(IUnitOfWork unitOfWork)
        {
			_unitOfWork = unitOfWork;
		}
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var UserIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(p => p.AppUserId == UserIdClaim.Value,
                includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            foreach (var cart in ShoppingCartVM.CartsList)
            {
				cart.Price = GetPriceBasedOnQuantity(cart.Count,
                    cart.Product.Price,
                    cart.Product.Price50,
                    cart.Product.Price100);

                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}
			return View(ShoppingCartVM);
        }

		// GET
		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var UserIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			ShoppingCartVM = new ShoppingCartVM()
			{
				CartsList = _unitOfWork.ShoppingCart.GetAll(p => p.AppUserId == UserIdClaim.Value,
				includeProperties: "Product"),
				OrderHeader = new OrderHeader()
			};

			ShoppingCartVM.OrderHeader.AppUser = _unitOfWork.AppUser.GetFirstOrDefault(x => x.Id == UserIdClaim.Value);

			ShoppingCartVM.OrderHeader.UserName = ShoppingCartVM.OrderHeader.AppUser.Name;
			ShoppingCartVM.OrderHeader.AppUserId = ShoppingCartVM.OrderHeader.AppUser.Id;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.AppUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.AppUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.AppUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.AppUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.AppUser.PostalCode;

			foreach (var cart in ShoppingCartVM.CartsList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count,
					cart.Product.Price,
					cart.Product.Price50,
					cart.Product.Price100);

				ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}
			return View(ShoppingCartVM);
		}

		// POST
		[ActionName("Summary")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var UserIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM.CartsList = _unitOfWork.ShoppingCart.GetAll(p => p.AppUserId == UserIdClaim.Value,
				includeProperties: "Product");

			ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			ShoppingCartVM.OrderHeader.AppUserId = UserIdClaim.Value;
			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

			foreach (var cart in ShoppingCartVM.CartsList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count,
					cart.Product.Price,
					cart.Product.Price50,
					cart.Product.Price100);

				ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}

			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			foreach (var cart in ShoppingCartVM.CartsList)
			{
				OrderDetail orderDetail = new OrderDetail()
				{
					ProductId = cart.ProductId,
					OrderId = ShoppingCartVM.OrderHeader.Id,
					Count = cart.Count,
					Price = cart.Price
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}

			_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.CartsList);
			_unitOfWork.Save();

			return RedirectToAction("Index", "Home");
		}

		public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(p => p.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
		public IActionResult Minus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(p => p.Id == cartId);
            if(cart.Count <= 1)
            {
				_unitOfWork.ShoppingCart.Remove(cart);
			}
            else
            {
				_unitOfWork.ShoppingCart.DecrementCount(cart, 1);
			}
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Remove(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(p => p.Id == cartId);
			_unitOfWork.ShoppingCart.Remove(cart);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		private double GetPriceBasedOnQuantity(int quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;

			}
            else
            {
				if (quantity <= 100)
				{
					return price50;

				}
                return price100;
			}
        }
    }
}
