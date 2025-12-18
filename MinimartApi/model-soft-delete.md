| Model             | Soft Delete? | Reason             |
| ----------------- | ------------ | ------------------ |
| User              |   Yes        | Disable login      |
| Role              |   Yes        | Retire roles       |
| UserRole          |   No         | Join table         |
| Customer          |   Yes        | Keep sales         |
| Category          |   Yes        | Product integrity  |
| Product           |   Yes        | Discontinued items |
| Supplier          |   Yes        | Purchase history   |
| Stock             |   No         | State table        |
| StockTransaction  |   No         | Audit              |
| Sale              |   No         | Accounting         |
| SaleItem          |   No         | Accounting         |
| PurchaseOrder     |   No         | Accounting         |
| PurchaseOrderItem |   No         | Accounting         |
