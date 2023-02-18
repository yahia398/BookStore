using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
		private readonly AppDbContext _db;
		public OrderHeaderRepository(AppDbContext db): base(db)
		{
			_db = db;
		}

		public void Update(OrderHeader orderHeader)
		{
			_db.OrderHeaders.Update(orderHeader);
		}

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var objFromDb = _db.OrderHeaders.Find(id);
			if(objFromDb != null)
			{
				objFromDb.OrderStatus= orderStatus;
				if(paymentStatus != null)
				{
					objFromDb.PaymentStatus= paymentStatus;
				}
			}
		}
		public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
            var objFromDb = _db.OrderHeaders.Find(id);
			if(objFromDb != null)
			{
				objFromDb.SessionId = sessionId;
				objFromDb.PaymentIntentId = paymentIntentId;
			}
        }
    }
}
