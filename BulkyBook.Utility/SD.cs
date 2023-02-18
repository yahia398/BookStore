using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public static class SD // SD: Static Details
    {
        // Identity Roles for the Application users
        public const string Role_User_Individual = "Individual";

        public const string Role_User_Company = "Company";

        public const string Role_User_Admin = "Admin";

        public const string Role_User_Employee = "Employee";


		// Order Status
		public const string OrderStatusPending = "Pending";       // After the order has been created

		public const string OrderStatusApproved = "Approved";     // After approving the customer order

		public const string OrderStatusInProcess = "Processing";  // When the order is processing

		public const string OrderStatusShipped = "Shipped";       // When the order shipped

		public const string OrderStatusCancelled = "Cancelled";   // The Order is cancelled

		public const string OrderStatusRefunded = "Refunded";     // The order is refunded


		// Payment Status
		public const string PaymentStatusPending = "Pending";                            // Initial Status

		public const string PaymentStatusApproved = "Approved";                          // After the payment hass been done successfully

		public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";   // If it is a company

		public const string PaymentStatusRejected = "Rejected";                          // it the payment has been rejected

	}
}
