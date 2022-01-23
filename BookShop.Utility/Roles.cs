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
    Cancelled,
    Refunded
}

public enum PaymentStatus
{
    Pending,
    Approved,
    DelayedPayment,
    Rejected
}