using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using zenvy.application.Interfaces.Repositories;
using zenvy.infrastructure.Persistence.SqlServer.ADO.net.Repository;
using zenvy.infrastructure.Persistence.SqlServer.EF.Repository;
using zenvy.infrastructure.UnitOfWork;
using zenvy.Application.Auth;
using zenvy.application.Interfaces;
using zenvy.application.Interfaces.Services;
using zenvy.infrastructure.Service;
using zenvy.Application.Interfaces.Repositories;
using zenvy.Infrastructure.Persistence.SQLServer.ADO.net.Repository;
using zenvy.infrastructure.Persistence.SqlServer.ADO;
using zenvy.infrastructure.persistence.sqlserver.ado.net.repository;
using zenvy.infrastructure.persistence.sqlserver.ado.net.repository.products;

namespace zenvy.infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DataProvider"];
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer =
                        configuration["Jwt:Issuer"],

                    ValidAudience =
                        configuration["Jwt:Audience"],

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                configuration["Jwt:Key"]!))
                };
        });

        if (provider == "ADO")
        {
            // 1. Fetch your connection string directly from the central appsettings.json file
            //var connectionString = configuration.GetConnectionString("DefaultConnection")
            //    ?? throw new ArgumentException("Connection string for DefaultConnection not found in configuration.");

            // 2. Register ADO repositories as Scoped, passing the request-specific connection string.
            // This guarantees a brand new connection instance is instantiated fresh with every HTTP request!
            services.AddScoped<IUserRepository, UserRepositories>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBrandRepository,BrandRepository>();
            services.AddScoped<IProductsRepository, ProductsRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IProfitRepository, ProfitRepository>();
            services.AddScoped<IEmployeeCommissionRepository, EmployeeCommissionRepository>();
            services.AddScoped<IInvestorRepository, InvestorRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            // Ensure your Unit of Work also accepts the string if it opens transactions
            services.AddScoped<IUnitOfWork, AdoUnitOfWork>();
        }
        else
        {
            // EF Core natively registers its context and connections as Scoped per request out of the box
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, EfUserRepositories>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        }

        return services;
    }
}
