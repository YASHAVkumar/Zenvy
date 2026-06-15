using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using zenvy.application.Interfaces.Services;
using zenvy.application.Service;
using zenvy.application.Services;
using zenvy.Application.Auth;
using zenvy.Application.Interfaces.Services;
using zenvy.Application.Services;

namespace zenvy.application
{
    public static class DependencyInjection
    {
        public static IServiceCollection
            AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IUserService,UserService>();
            services.AddScoped<ICategoryService,CategoryService>();
            services.AddScoped<IAuthService,AuthService>();
            services.AddScoped<IBrandService,BrandService>();
            services.AddScoped<IProductService,ProductService>();
            services.AddScoped<IWarehouseService,WarehouseService>();
            services.AddScoped<IInventoryService,InventoryService>();
            services.AddScoped<ISupplierService,SupplierService>();
            services.AddScoped<ICustomerService,CustomerService>();
            services.AddScoped<IPurchaseOrderService,PurchaseOrderService>();
            services.AddScoped<ISalesOrderService,SalesOrderService>();
            services.AddScoped<IPaymentService,PaymentService>();
            services.AddScoped<IShipmentService,ShipmentService>();
            return services;
        }
    }
}
