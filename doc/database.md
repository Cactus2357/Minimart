Excellent — your answers are clear enough to design a **production-ready Minimart / e-commerce database** without guessing.
Below is a **clean, scalable redesign** aligned with **ASP.NET Core + SQL Server**, medium scale, audit-safe, and approval-driven.

---

# 🧩 1️⃣ High-Level ERD (Textual)

```
User ──< UserRole >── Role
User ──< Address
User ──< Order ──< OrderItem >── ProductVariant ── Product
                                   │
                                   └── PriceHistory

Category (self-referencing)

Order ── Payment
Order ──< OrderStatusHistory

Product ──< ProductApproval
Order ──< OrderApproval
```

---

# 👤 2️⃣ Identity & Authorization

### Users

```sql
Users (
    UserId UNIQUEIDENTIFIER PK,
    Username NVARCHAR(100) UNIQUE,
    Email NVARCHAR(255) UNIQUE,
    PasswordHash NVARCHAR(MAX),
    IsDeleted BIT DEFAULT 0,

    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
)
```

### Roles (Admin / Staff / Customer)

```sql
Roles (
    RoleId INT PK,
    Name NVARCHAR(50) UNIQUE
)

UserRoles (
    UserId UNIQUEIDENTIFIER FK -> Users,
    RoleId INT FK -> Roles,
    PRIMARY KEY (UserId, RoleId)
)
```

---

# 🗂️ 3️⃣ Category (Multi-Level, Soft Delete)

```sql
Categories (
    CategoryId INT PK,
    Name NVARCHAR(200),
    ParentCategoryId INT NULL FK -> Categories,
    IsDeleted BIT DEFAULT 0,

    CreatedBy UNIQUEIDENTIFIER FK -> Users,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
)
```

✔ Supports unlimited nesting
✔ Admin + Staff controlled

---

# 📦 4️⃣ Product & Variants

### Product

```sql
Products (
    ProductId INT PK,
    Name NVARCHAR(255),
    Description NVARCHAR(MAX),
    Status NVARCHAR(50), -- Pending, Approved, Rejected
    IsDeleted BIT DEFAULT 0,

    CreatedBy UNIQUEIDENTIFIER FK -> Users, -- Staff
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
)
```

### Product Variant (Stock tracked here)

```sql
ProductVariants (
    VariantId INT PK,
    ProductId INT FK -> Products,
    SKU NVARCHAR(100) UNIQUE,
    VariantName NVARCHAR(255), -- Size M / Color Red
    Stock INT CHECK (Stock >= 0),

    CreatedAt DATETIME2
)
```

---

# 💰 5️⃣ Pricing & Discount History

```sql
PriceHistory (
    PriceHistoryId INT PK,
    VariantId INT FK -> ProductVariants,
    OriginalPrice DECIMAL(18,2),
    SalePrice DECIMAL(18,2),
    DiscountPercent INT,

    EffectiveFrom DATETIME2,
    EffectiveTo DATETIME2 NULL
)
```

✔ Supports **price change tracking**
✔ Safe for audits & reports

---

# 🛒 6️⃣ Orders

### Order

```sql
Orders (
    OrderId INT PK,
    CustomerId UNIQUEIDENTIFIER FK -> Users,
    AddressId INT FK -> Addresses,
    CurrentStatus NVARCHAR(50),
    TotalAmount DECIMAL(18,2),

    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
)
```

### Order Items (variants only)

```sql
OrderItems (
    OrderItemId INT PK,
    OrderId INT FK -> Orders,
    VariantId INT FK -> ProductVariants,
    Quantity INT CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2)
)
```

✔ One order → many variants
✔ Same variant allowed multiple times (quantity)

---

# 🔄 7️⃣ Order Status History (Mandatory)

```sql
OrderStatusHistory (
    HistoryId INT PK,
    OrderId INT FK -> Orders,
    OldStatus NVARCHAR(50),
    NewStatus NVARCHAR(50),
    ChangedBy UNIQUEIDENTIFIER FK -> Users,
    ChangedAt DATETIME2
)
```

✔ Tracks **every status change**
✔ Required for refunds & disputes

---

# 💳 8️⃣ Payment (Exactly One)

```sql
Payments (
    PaymentId INT PK,
    OrderId INT UNIQUE FK -> Orders,
    Method NVARCHAR(50), -- COD / VNPay / Stripe / PayPal
    ProviderTransactionId NVARCHAR(255),
    Amount DECIMAL(18,2),
    Status NVARCHAR(50), -- Pending, Paid, Refunded

    CreatedAt DATETIME2
)
```

✔ Full refund supported
✔ Online & COD compatible

---

# 🏠 9️⃣ Address (Linked, Not Snapshot)

```sql
Addresses (
    AddressId INT PK,
    UserId UNIQUEIDENTIFIER FK -> Users,
    ReceiverName NVARCHAR(255),
    Phone NVARCHAR(50),
    AddressLine NVARCHAR(500),
    IsDefault BIT DEFAULT 0,

    CreatedAt DATETIME2
)
```

---

# ✅ 1️⃣0️⃣ Approval Workflow (Separate History)

### Product Approval

```sql
ProductApprovals (
    ApprovalId INT PK,
    ProductId INT FK -> Products,
    Action NVARCHAR(50), -- Create / Update
    Status NVARCHAR(50), -- Pending / Approved / Rejected
    ApprovedBy UNIQUEIDENTIFIER FK -> Users,
    ApprovedAt DATETIME2
)
```

### Order Approval (Cancel / Refund)

```sql
OrderApprovals (
    ApprovalId INT PK,
    OrderId INT FK -> Orders,
    Action NVARCHAR(50), -- Cancel / Refund
    Status NVARCHAR(50),
    ApprovedBy UNIQUEIDENTIFIER FK -> Users,
    ApprovedAt DATETIME2
)
```

✔ Admin + Staff allowed
✔ Fully auditable

---

# 🧠 1️⃣1️⃣ Why This Design Works

✔ Variant-based stock (no overselling)
✔ Soft delete where required (Product, Category, User)
✔ Price & status **history preserved**
✔ Approval workflows isolated (no polluted main tables)
✔ ASP.NET Identity compatible
✔ Scales safely to **100k users**

---

## 🚀 Next (Optional)

If you want, I can:

* Generate **EF Core models + Fluent API**
* Draw **visual ERD**
* Add **indexes & performance tuning**
* Map this to **CQRS / DTO patterns**

Just tell me what to do next.
