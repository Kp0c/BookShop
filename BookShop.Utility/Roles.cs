namespace BookShop.Utility;

public enum Roles
{
    Admin,
    Employee,
    Individual,
    Company
}

public enum Status
{
    Pending,
    Approved,
    Processing,
    Shipped,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Approved,
    DelayedPayment,
    Rejected,
    Refunded,
    Cancelled
}