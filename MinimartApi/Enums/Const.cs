namespace MinimartApi.Enums
{
    public class Const
    {
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_STAFF = "Staff";
        public const string ROLE_CUSTOMER = "Customer";

        // ------------- //

        public const string ORDER_STATUS_PENDING = "Pending";
        public const string ORDER_STATUS_PAID = "Paid";
        public const string ORDER_STATUS_PROCESSING = "Processing";
        public const string ORDER_STATUS_SHIPPED = "Shipped";
        public const string ORDER_STATUS_COMPLETED = "Completed";
        public const string ORDER_STATUS_CANCELLED = "Cancelled";
        public const string ORDER_STATUS_REFUNDED = "Refunded";

        // ------------- //

        public const string PAYMENT_STATUS_PENDING = "Pending";
        public const string PAYMENT_STATUS_PAID = "Paid";
        public const string PAYMENT_STATUS_REFUNDED = "Refunded";
        public const string PAYMENT_STATUS_FAILED = "Failed";

        // ------------- //

        public const string PRODUCT_STATUS_DRAFT = "Draft";
        public const string PRODUCT_STATUS_PENDING = "Pending";
        public const string PRODUCT_STATUS_APPROVED = "Approved";
        public const string PRODUCT_STATUS_REJECTED = "Rejected";
        public const string PRODUCT_STATUS_SUSPENDED = "Suspended";
        public const string PRODUCT_STATUS_ARCHIVED = "Archived";

        // ------------- //

        public const string BUCKET_PRODUCT = "products";
        public const string BUCKET_PRODUCT_VARIANTS = "variants";

    }
}
