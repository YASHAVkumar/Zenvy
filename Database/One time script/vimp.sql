SELECT * FROM sys.tables;

SELECT * FROM Shipments            	 (nolock)
SELECT * FROM SupplierAddress		 (nolock)
SELECT * FROM Suppliers				 (nolock)
SELECT * FROM Expenses				 (nolock)
SELECT * FROM Warehouses			 (nolock)
SELECT * FROM SalesReturnLines		 (nolock)
SELECT * FROM ApiAuditLogs			 (nolock)
SELECT * FROM InventoryTransactions	 (nolock)
SELECT * FROM SalesOrders			 (nolock)
SELECT * FROM EmployeeCommissions	 (nolock)
SELECT * FROM PurchaseOrders		 (nolock)
SELECT * FROM Brands				 (nolock)
SELECT * FROM Categories			 (nolock)
SELECT * FROM Customers				 (nolock)
SELECT * FROM SalesReturns			 (nolock)
SELECT * FROM ExpenseTypes			 (nolock)
SELECT * FROM Inventory				 (nolock)
SELECT * FROM Investors				 (nolock)
SELECT * FROM InvestorTransactions	 (nolock)
SELECT * FROM Users					 (nolock)
SELECT * FROM MarketplaceSettlements (nolock)
SELECT * FROM PaymentMethods		 (nolock)
SELECT * FROM Payments				 (nolock)
SELECT * FROM Prices				 (nolock)
SELECT * FROM ProductMasters		 (nolock)
SELECT * FROM Products				 (nolock)
SELECT * FROM ProductImages			 (nolock)
SELECT * FROM ProductVariants		 (nolock)
SELECT * FROM ProfitDistributions	 (nolock)
SELECT * FROM PurchaseOrderLines	 (nolock)
SELECT * FROM InventoryLedger		 (nolock)
SELECT * FROM Employees				 (nolock)
SELECT * FROM Roles					 (nolock)
SELECT * FROM Salary				 (nolock)
SELECT * FROM SalesChannels			 (nolock)
SELECT * FROM SalesOrderLines		 (nolock)
SELECT * FROM Attendance			 (nolock)
SELECT * FROM Tasks					 (nolock)

SELECT
    name,
    create_date,
    modify_date
FROM sys.procedures
ORDER BY name;

sp_helptext usp_AddSalesReturnLine
sp_helptext usp_AdjustInventory
sp_helptext usp_CreateCustomer
sp_helptext usp_CreateEmployeeCommission
sp_helptext usp_CreateExpense
sp_helptext usp_CreateExpenseType
sp_helptext usp_CreateInventory
sp_helptext usp_CreateInvestor
sp_helptext usp_CreateMarketplaceSettlement
sp_helptext usp_CreatePayment
sp_helptext usp_CreateProduct
sp_helptext usp_CreatePurchaseOrder
sp_helptext usp_CreateSalesChannel
sp_helptext usp_CreateSalesOrder
sp_helptext usp_CreateSalesReturn
sp_helptext usp_CreateShipment
sp_helptext usp_CreateSupplier
sp_helptext usp_CreateWarehouse
sp_helptext usp_DamageInventory
sp_helptext usp_DeleteSalesChannel
sp_helptext usp_DeleteWarehouse
sp_helptext usp_DistributeInvestorProfit
sp_helptext usp_GetApiAuditLogs
sp_helptext usp_GetChannelAnalytics
sp_helptext usp_GetCustomers
sp_helptext usp_GetDashboardAnalytics
sp_helptext usp_GetEmployeeCommissions
sp_helptext usp_GetEmployeeDashboard
sp_helptext usp_GetExecutiveDashboard
sp_helptext usp_GetExpenses
sp_helptext usp_GetExpenseTypes
sp_helptext usp_GetFinanceDashboard
sp_helptext usp_GetInventory
sp_helptext usp_GetInventoryById
sp_helptext usp_GetInventoryTransactions
sp_helptext usp_GetInvestorDashboard
sp_helptext usp_GetInvestors
sp_helptext usp_GetMarketplaceDashboard
sp_helptext usp_GetMarketplaceSettlementById
sp_helptext usp_GetMarketplaceSettlements
sp_helptext usp_GetOperationsDashboard
sp_helptext usp_GetPayments
sp_helptext usp_GetProfitDistributions
sp_helptext usp_GetProfitSourceData
sp_helptext usp_GetPurchaseOrderById
sp_helptext usp_GetPurchaseOrders
sp_helptext usp_GetSalesChannelById
sp_helptext usp_GetSalesChannels
sp_helptext usp_GetSalesOrderById
sp_helptext usp_GetSalesOrders
sp_helptext usp_GetSalesReturnById
sp_helptext usp_GetSalesReturns
sp_helptext usp_GetShipments
sp_helptext usp_GetSuppliers
sp_helptext usp_GetWarehouseById
sp_helptext usp_GetWarehouseDropdown
sp_helptext usp_GetWarehouses
sp_helptext usp_ProcessSalesReturn
sp_helptext usp_TransferInventory
sp_helptext usp_UpdateInventory
sp_helptext usp_UpdateSalesChannel
sp_helptext usp_UpdateSalesReturnStatus
sp_helptext usp_UpdateWarehouse
sp_helptext usp_WriteApiAuditLog