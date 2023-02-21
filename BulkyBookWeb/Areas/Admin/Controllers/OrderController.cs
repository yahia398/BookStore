using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
		[BindProperty]
		public OrderVM OrderVM { get; set; }
		public OrderController(IUnitOfWork unitOfWork, IEmailSender emailSender)
		{
			_unitOfWork = unitOfWork;
            _emailSender = emailSender;
		}

		public IActionResult Index()
		{
			return View();
		}
		// GET
        public IActionResult Details(int orderId)
        {
			OrderVM = new OrderVM()
			{
				OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderId, includeProperties: "AppUser"),
				OrderDetail = _unitOfWork.OrderDetail.GetAll(x => x.OrderId == orderId, includeProperties: "Product")
			};
            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_Pay_Now()
        {
			OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id, includeProperties: "AppUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(x => x.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            // Stripe
            var domain = "https://localhost:44348/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                // Represents all the items that the customer have in the shopping cart
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"Admin/Order/Details?orderId={OrderVM.OrderHeader.Id}",
            };

            foreach (var item in OrderVM.OrderDetail)
            {

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == orderId, includeProperties: "AppUser");
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                // Check the stripe status and make sure that payment is successful
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderId, orderHeader.SessionId, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                _emailSender.SendEmailAsync(orderHeader.AppUser.Email, "Payment Done !", "<p>Congratulations! You have been paid successfully for your order");
            }
            return View(orderId);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_User_Admin + "," + SD.Role_User_Employee)]
        public IActionResult UpdateOrderDetail()
        {
			var objFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id, tracked: false);
			objFromDb.UserName = OrderVM.OrderHeader.UserName;
			objFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
			objFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
			objFromDb.City = OrderVM.OrderHeader.City;
			objFromDb.State = OrderVM.OrderHeader.State;
			objFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

			if (OrderVM.OrderHeader.Carrier != null)
			{
				objFromDb.Carrier = OrderVM.OrderHeader.Carrier;
			}
			if (OrderVM.OrderHeader.TrackingNumber != null)
			{
				objFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			}
			_unitOfWork.OrderHeader.Update(objFromDb);
			_unitOfWork.Save();
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id});
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_User_Admin + "," + SD.Role_User_Employee)]
        public IActionResult StartProcessing()
        {
			_unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.OrderStatusInProcess);
            _unitOfWork.Save();
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_User_Admin + "," + SD.Role_User_Employee)]
        public IActionResult ShipOrder()
        {
			var objFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id, tracked: false);
			objFromDb.Carrier = OrderVM.OrderHeader.Carrier;
			objFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			objFromDb.ShippingDate = DateTime.Now;
			objFromDb.OrderStatus = SD.OrderStatusShipped;
			_unitOfWork.OrderHeader.Update(objFromDb);
			_unitOfWork.Save();
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_User_Admin + "," + SD.Role_User_Employee)]
        public IActionResult CancelOrder()
        {
			var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id, tracked: false);
			if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
			{
				var options = new RefundCreateOptions()
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeader.PaymentIntentId
				};

				var service = new RefundService();
				Refund refund = service.Create(options);
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.OrderStatusCancelled, SD.PaymentStatusRefunded);
			}
			else
			{
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.OrderStatusCancelled, SD.PaymentStatusRejected);
            }
            _unitOfWork.Save();
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeader;

            if (User.IsInRole(SD.Role_User_Admin) || User.IsInRole(SD.Role_User_Employee))
			{
                orderHeader = _unitOfWork.OrderHeader.GetAll(includeProperties: "AppUser");
            }
			else
			{
				var claimsIdentity = (ClaimsIdentity) User.Identity;
				var UserIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeader = _unitOfWork.OrderHeader.GetAll(x => x.AppUserId == UserIdClaim.Value, includeProperties: "AppUser");
            }
			
			switch (status)
			{
				case "pending":
					orderHeader = orderHeader.Where(x => x.PaymentStatus == SD.PaymentStatusDelayedPayment);
					break;
                case "approved":
                    orderHeader = orderHeader.Where(x => x.OrderStatus == SD.OrderStatusApproved);
                    break;
                case "inprocess":
                    orderHeader = orderHeader.Where(x => x.OrderStatus == SD.OrderStatusInProcess);
                    break;
                case "completed":
                    orderHeader = orderHeader.Where(x => x.OrderStatus == SD.OrderStatusShipped);
                    break;
				default:
                    break;

            }
			return Json(new { data = orderHeader });
		}
		#endregion
	}
}
