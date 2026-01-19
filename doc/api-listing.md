Below is a **complete, clean, and realistic REST API endpoint list** for a **Minimart Management System**, aligned with what you’ve already built (users, roles, products, stock, sales, soft-delete, admin use cases).

I’ll group them by **domain**, mark **admin-only**, and assume:

* JWT auth
* Soft-delete (`IsDeleted`)
* Global query filters
* Admin can see deleted data via flag or `IgnoreQueryFilters()`

---

## 1. Auth & Account

**Public / Authenticated**

| Method | Endpoint                         | Description          |
| ------ | -------------------------------- | -------------------- |
| POST   | `/api/auth/register`             | Register user        |
| POST   | `/api/auth/login`                | Login, get JWT       |
| POST   | `/api/auth/refresh`              | Refresh token        |
| POST   | `/api/auth/logout`               | Logout               |
| POST   | `/api/auth/confirm-email`        | Confirm email        |
| POST   | `/api/auth/resend-confirm-email` | Resend confirmation  |
| POST   | `/api/auth/forgot-password`      | Send reset code      |
| POST   | `/api/auth/reset-password`       | Reset password       |
| GET    | `/api/auth/me`                   | Current user profile |

---

## 2. Users (Admin)

**Admin-only**

| Method | Endpoint                         | Description          |
| ------ | -------------------------------- | -------------------- |
| GET    | `/api/users`                     | List users           |
| GET    | `/api/users/{id}`                | Get user by id       |
| POST   | `/api/users`                     | Create user          |
| PUT    | `/api/users/{id}`                | Update user          |
| DELETE | `/api/users/{id}`                | Soft-delete user     |
| PATCH  | `/api/users/{id}/restore`        | Restore user         |
| GET    | `/api/users?includeDeleted=true` | Include soft-deleted |

---

## 3. Roles & Permissions

**Admin-only**

| Method | Endpoint                 | Description         |
| ------ | ------------------------ | ------------------- |
| GET    | `/api/roles`             | List roles          |
| POST   | `/api/roles`             | Create role         |
| PUT    | `/api/roles/{id}`        | Update role         |
| DELETE | `/api/roles/{id}`        | Soft-delete role    |
| POST   | `/api/roles/{id}/assign` | Assign role to user |
| POST   | `/api/roles/{id}/revoke` | Revoke role         |

---

## 4. Categories

**Admin / Staff**

| Method | Endpoint                              | Description     |
| ------ | ------------------------------------- | --------------- |
| GET    | `/api/categories`                     | List categories |
| GET    | `/api/categories/{id}`                | Get category    |
| POST   | `/api/categories`                     | Create category |
| PUT    | `/api/categories/{id}`                | Update category |
| DELETE | `/api/categories/{id}`                | Soft-delete     |
| PATCH  | `/api/categories/{id}/restore`        | Restore         |
| GET    | `/api/categories?includeDeleted=true` | Admin view      |

---

## 5. Products

**Admin / Staff**

| Method | Endpoint                            | Description     |
| ------ | ----------------------------------- | --------------- |
| GET    | `/api/products`                     | List products   |
| GET    | `/api/products/{id}`                | Get product     |
| POST   | `/api/products`                     | Create product  |
| PUT    | `/api/products/{id}`                | Update product  |
| DELETE | `/api/products/{id}`                | Soft-delete     |
| PATCH  | `/api/products/{id}/restore`        | Restore         |
| GET    | `/api/products/low-stock`           | Low stock alert |
| GET    | `/api/products?includeDeleted=true` | Admin view      |

---

## 6. Suppliers

**Admin / Staff**

| Method | Endpoint                      | Description     |
| ------ | ----------------------------- | --------------- |
| GET    | `/api/suppliers`              | List suppliers  |
| GET    | `/api/suppliers/{id}`         | Get supplier    |
| POST   | `/api/suppliers`              | Create supplier |
| PUT    | `/api/suppliers/{id}`         | Update supplier |
| DELETE | `/api/suppliers/{id}`         | Soft-delete     |
| PATCH  | `/api/suppliers/{id}/restore` | Restore         |

---

## 7. Customers

**Staff**

| Method | Endpoint                      | Description     |
| ------ | ----------------------------- | --------------- |
| GET    | `/api/customers`              | List customers  |
| GET    | `/api/customers/{id}`         | Get customer    |
| POST   | `/api/customers`              | Create customer |
| PUT    | `/api/customers/{id}`         | Update customer |
| DELETE | `/api/customers/{id}`         | Soft-delete     |
| PATCH  | `/api/customers/{id}/restore` | Restore         |

---

## 8. Stock & Inventory

**Admin / Staff**

| Method | Endpoint                  | Description       |
| ------ | ------------------------- | ----------------- |
| GET    | `/api/stocks`             | View stock levels |
| GET    | `/api/stocks/{productId}` | Product stock     |
| POST   | `/api/stocks/adjust`      | Manual adjustment |
| GET    | `/api/stock-transactions` | Stock history     |

---

## 9. Purchase Orders (Inbound)

**Admin / Staff**

| Method | Endpoint                            | Description   |
| ------ | ----------------------------------- | ------------- |
| GET    | `/api/purchase-orders`              | List orders   |
| GET    | `/api/purchase-orders/{id}`         | Get order     |
| POST   | `/api/purchase-orders`              | Create PO     |
| POST   | `/api/purchase-orders/{id}/receive` | Receive goods |
| PUT    | `/api/purchase-orders/{id}`         | Update        |
| DELETE | `/api/purchase-orders/{id}`         | Cancel (soft) |

---

## 10. Sales (POS)

**Staff**

| Method | Endpoint                 | Description  |
| ------ | ------------------------ | ------------ |
| POST   | `/api/sales`             | Create sale  |
| GET    | `/api/sales`             | List sales   |
| GET    | `/api/sales/{id}`        | Sale details |
| POST   | `/api/sales/{id}/refund` | Refund sale  |

---

## 11. Reports & Analytics

**Admin**

| Method | Endpoint                       | Description     |
| ------ | ------------------------------ | --------------- |
| GET    | `/api/reports/daily-sales`     | Daily sales     |
| GET    | `/api/reports/monthly-sales`   | Monthly sales   |
| GET    | `/api/reports/top-products`    | Best sellers    |
| GET    | `/api/reports/inventory-value` | Inventory value |
| GET    | `/api/reports/profit`          | Profit report   |

---

## 12. Admin / System

**Admin-only**

| Method | Endpoint                | Description          |
| ------ | ----------------------- | -------------------- |
| GET    | `/api/admin/audit-logs` | Audit trail          |
| GET    | `/api/admin/deleted`    | All soft-deleted     |
| POST   | `/api/admin/restore`    | Restore entity       |
| DELETE | `/api/admin/purge`      | Hard delete (danger) |

---

## Recommended Query Flags (consistent)

```http
?includeDeleted=true
?from=2025-01-01&to=2025-01-31
?page=1&pageSize=20
```

---

## Why this design is solid

✅ Matches EF Core soft-delete
✅ Prevents cascade delete issues
✅ Clear admin vs staff separation
✅ Scales to real POS usage
✅ Easy frontend mapping

---

If you want next:

* ✅ **Swagger grouping**
* ✅ **Controller templates**
* ✅ **RBAC matrix (who can access what)**
* ✅ **Minimal version for school submission**

Just say which one.
