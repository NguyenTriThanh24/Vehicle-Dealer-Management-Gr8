using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Models;
using System.Security.Cryptography;
using System.Text;

namespace Vehicle_Dealer_Management.DAL.Data
{
    public static class DbSeeder
    {
        // Helper method để hash password
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static void SeedData(ApplicationDbContext context)
        {
            // Đảm bảo database đã được tạo
            context.Database.EnsureCreated();

            // Seed Roles nếu chưa có
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Code = "CUSTOMER", Name = "Khách hàng", IsOperational = true },
                    new Role { Code = "DEALER_STAFF", Name = "Nhân viên đại lý", IsOperational = true },
                    new Role { Code = "DEALER_MANAGER", Name = "Quản lý đại lý", IsOperational = true },
                    new Role { Code = "EVM_STAFF", Name = "Nhân viên hãng xe", IsOperational = true },
                    new Role { Code = "EVM_ADMIN", Name = "Quản trị viên", IsOperational = true }
                };

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            // Seed Dealers
            if (!context.Dealers.Any())
            {
                var dealers = new List<Dealer>
                {
                    new Dealer
                    {
                        Code = "DL001",
                        Name = "Đại lý Hà Nội",
                        Address = "123 Đường Láng, Đống Đa, Hà Nội",
                        PhoneNumber = "0241234567",
                        Email = "hanoi@dealer.com",
                        Status = "ACTIVE",
                        IsActive = true
                    },
                    new Dealer
                    {
                        Code = "DL002",
                        Name = "Đại lý TP.HCM",
                        Address = "456 Nguyễn Huệ, Quận 1, TP.HCM",
                        PhoneNumber = "0289876543",
                        Email = "hcm@dealer.com",
                        Status = "ACTIVE",
                        IsActive = true
                    }
                };

                context.Dealers.AddRange(dealers);
                context.SaveChanges();
            }

            // Seed Users (5 users tương ứng 5 roles)
            if (!context.Users.Any())
            {
                var roles = context.Roles.ToList();
                var dealers = context.Dealers.ToList();

                var users = new List<User>
                {
                    // Customer
                    new User
                    {
                        Email = "customer@test.com",
                        PasswordHash = HashPassword("123456"),
                        FullName = "Nguyễn Văn Khách",
                        Phone = "0901234567",
                        RoleId = roles.First(r => r.Code == "CUSTOMER").Id,
                        DealerId = null,
                        CreatedAt = DateTime.UtcNow
                    },
                    // Dealer Staff
                    new User
                    {
                        Email = "dealerstaff@test.com",
                        PasswordHash = HashPassword("123456"),
                        FullName = "Trần Thị Nhân Viên",
                        Phone = "0902345678",
                        RoleId = roles.First(r => r.Code == "DEALER_STAFF").Id,
                        DealerId = dealers.First().Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    // Dealer Manager
                    new User
                    {
                        Email = "dealermanager@test.com",
                        PasswordHash = HashPassword("123456"),
                        FullName = "Lê Văn Quản Lý",
                        Phone = "0903456789",
                        RoleId = roles.First(r => r.Code == "DEALER_MANAGER").Id,
                        DealerId = dealers.First().Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    // EVM Staff
                    new User
                    {
                        Email = "evmstaff@test.com",
                        PasswordHash = HashPassword("123456"),
                        FullName = "Phạm Thị Nhân Viên Hãng",
                        Phone = "0904567890",
                        RoleId = roles.First(r => r.Code == "EVM_STAFF").Id,
                        DealerId = null,
                        CreatedAt = DateTime.UtcNow
                    },
                    // EVM Admin
                    new User
                    {
                        Email = "admin@test.com",
                        PasswordHash = HashPassword("123456"),
                        FullName = "Hoàng Văn Admin",
                        Phone = "0905678901",
                        RoleId = roles.First(r => r.Code == "EVM_ADMIN").Id,
                        DealerId = null,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // Seed Vehicles (3 vehicles)
            if (!context.Vehicles.Any())
            {
                var vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        ModelName = "Model S",
                        VariantName = "Premium",
                        SpecJson = @"{
                            ""battery"": ""100kWh"",
                            ""range"": ""610km"",
                            ""power"": ""670hp"",
                            ""acceleration"": ""3.1s (0-100km/h)"",
                            ""maxSpeed"": ""250km/h""
                        }",
                        ImageUrl = "https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800",
                        Status = "AVAILABLE",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Vehicle
                    {
                        ModelName = "Model 3",
                        VariantName = "Standard",
                        SpecJson = @"{
                            ""battery"": ""60kWh"",
                            ""range"": ""420km"",
                            ""power"": ""283hp"",
                            ""acceleration"": ""5.3s (0-100km/h)"",
                            ""maxSpeed"": ""225km/h""
                        }",
                        ImageUrl = "https://images.unsplash.com/photo-1617531653332-bd46c24f2068?w=800",
                        Status = "AVAILABLE",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Vehicle
                    {
                        ModelName = "Model X",
                        VariantName = "Performance",
                        SpecJson = @"{
                            ""battery"": ""100kWh"",
                            ""range"": ""576km"",
                            ""power"": ""780hp"",
                            ""acceleration"": ""2.6s (0-100km/h)"",
                            ""maxSpeed"": ""262km/h""
                        }",
                        ImageUrl = "https://giaxeoto.vn/admin/upload/images/resize/640-van-hanh-xe-tesla-model-x.jpg",
                        Status = "AVAILABLE",
                        CreatedDate = DateTime.UtcNow
                    }
                };

                context.Vehicles.AddRange(vehicles);
                context.SaveChanges();
            }

            // Seed Price Policies
            if (!context.PricePolicies.Any())
            {
                var vehicles = context.Vehicles.ToList();
                var dealers = context.Dealers.ToList();

                var pricePolicies = new List<PricePolicy>
                {
                    // Global price cho Model S
                    new PricePolicy
                    {
                        VehicleId = vehicles[0].Id,
                        DealerId = null,
                        Msrp = 2500000000, // 2.5 tỷ
                        WholesalePrice = 2300000000, // 2.3 tỷ
                        ValidFrom = DateTime.UtcNow.AddMonths(-1),
                        ValidTo = null,
                        CreatedDate = DateTime.UtcNow
                    },
                    // Global price cho Model 3
                    new PricePolicy
                    {
                        VehicleId = vehicles[1].Id,
                        DealerId = null,
                        Msrp = 1500000000, // 1.5 tỷ
                        WholesalePrice = 1400000000, // 1.4 tỷ
                        ValidFrom = DateTime.UtcNow.AddMonths(-1),
                        ValidTo = null,
                        CreatedDate = DateTime.UtcNow
                    },
                    // Global price cho Model X
                    new PricePolicy
                    {
                        VehicleId = vehicles[2].Id,
                        DealerId = null,
                        Msrp = 3500000000, // 3.5 tỷ
                        WholesalePrice = 3300000000, // 3.3 tỷ
                        ValidFrom = DateTime.UtcNow.AddMonths(-1),
                        ValidTo = null,
                        CreatedDate = DateTime.UtcNow
                    }
                };

                context.PricePolicies.AddRange(pricePolicies);
                context.SaveChanges();
            }

            // Seed Stocks (EVM stock)
            if (!context.Stocks.Any())
            {
                var vehicles = context.Vehicles.ToList();

                var stocks = new List<Stock>
                {
                    // EVM stock
                    new Stock
                    {
                        OwnerType = "EVM",
                        OwnerId = 0, // 0 = EVM
                        VehicleId = vehicles[0].Id,
                        ColorCode = "BLACK",
                        Qty = 10,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Stock
                    {
                        OwnerType = "EVM",
                        OwnerId = 0,
                        VehicleId = vehicles[0].Id,
                        ColorCode = "WHITE",
                        Qty = 8,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Stock
                    {
                        OwnerType = "EVM",
                        OwnerId = 0,
                        VehicleId = vehicles[1].Id,
                        ColorCode = "BLACK",
                        Qty = 15,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Stock
                    {
                        OwnerType = "EVM",
                        OwnerId = 0,
                        VehicleId = vehicles[1].Id,
                        ColorCode = "RED",
                        Qty = 12,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Stock
                    {
                        OwnerType = "EVM",
                        OwnerId = 0,
                        VehicleId = vehicles[2].Id,
                        ColorCode = "BLACK",
                        Qty = 5,
                        CreatedDate = DateTime.UtcNow
                    }
                };

                context.Stocks.AddRange(stocks);
                context.SaveChanges();
            }

            // Seed Customer Profiles
            if (!context.CustomerProfiles.Any())
            {
                var customers = new List<CustomerProfile>
                {
                    new CustomerProfile
                    {
                        FullName = "Nguyễn Văn Khách",
                        Phone = "0901234567",
                        Email = "customer@test.com",
                        Address = "123 Đường ABC, Quận 1, TP.HCM",
                        CreatedDate = DateTime.UtcNow
                    },
                    new CustomerProfile
                    {
                        FullName = "Trần Thị Mai",
                        Phone = "0907654321",
                        Email = "mai.tran@example.com",
                        Address = "456 Đường XYZ, Quận 2, TP.HCM",
                        CreatedDate = DateTime.UtcNow
                    }
                };

                context.CustomerProfiles.AddRange(customers);
                context.SaveChanges();
            }

            // Seed Promotions (optional)
            if (!context.Promotions.Any())
            {
                var promotions = new List<Promotion>
                {
                    new Promotion
                    {
                        Name = "Giảm giá cuối năm",
                        Scope = "GLOBAL",
                        RuleJson = @"{""discountPercent"": 10}",
                        ValidFrom = DateTime.UtcNow,
                        ValidTo = DateTime.UtcNow.AddMonths(1),
                        CreatedDate = DateTime.UtcNow
                    }
                };

                context.Promotions.AddRange(promotions);
                context.SaveChanges();
            }
        }
    }
}

