namespace Rihla.Core.Enums
{
    public enum UserRole
    {
        SuperAdmin = 1,
        TenantAdmin = 2,
        SystemAdmin = 3,
        Driver = 4,
        Parent = 5,
        Student = 6,
        Dispatcher = 7,
        Maintenance = 8
    }

    public enum StudentStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Graduated = 4,
        Transferred = 5
    }

    public enum VehicleStatus
    {
        Active = 1,
        Inactive = 2,
        Maintenance = 3,
        OutOfService = 4,
        Retired = 5
    }

    public enum VehicleType
    {
        Bus = 1,
        MiniBus = 2,
        Van = 3,
        Car = 4,
        SpecialNeeds = 5
    }

    public enum DriverStatus
    {
        Active = 1,
        Inactive = 2,
        OnLeave = 3,
        Suspended = 4,
        Terminated = 5
    }

    public enum RouteStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        UnderReview = 4
    }

    public enum TripStatus
    {
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        Delayed = 5
    }

    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        Late = 3,
        Excused = 4,
        NoShow = 5
    }

    public enum Priority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4,
        Emergency = 5
    }

    public enum NotificationChannel
    {
        Email = 1,
        Sms = 2,
        Push = 3,
        InApp = 4,
        Voice = 5
    }

    public enum NotificationPriority
    {
        Normal = 1,
        High = 2,
        Urgent = 3,
        Critical = 4
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5,
        Refunded = 6
    }

    public enum PaymentType
    {
        OneTime = 1,
        Recurring = 2,
        Subscription = 3,
        Deposit = 4,
        Fee = 5
    }
}


    public enum TripType
    {
        PickUp = 1,
        DropOff = 2,
        FieldTrip = 3,
        Emergency = 4,
        Special = 5
    }

    public enum MaintenanceType
    {
        Preventive = 1,
        Corrective = 2,
        Emergency = 3,
        Inspection = 4,
        Repair = 5
    }

    public enum MaintenanceStatus
    {
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        Overdue = 5
    }


    public enum PaymentMethod
    {
        Cash = 1,
        CreditCard = 2,
        DebitCard = 3,
        BankTransfer = 4,
        Check = 5,
        OnlinePayment = 6,
        MobilePayment = 7
    }

    public enum RouteType
    {
        Regular = 1,
        Express = 2,
        SpecialNeeds = 3,
        FieldTrip = 4,
        Emergency = 5
    }

