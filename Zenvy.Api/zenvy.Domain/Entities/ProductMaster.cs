namespace zenvy.domain.Entities
{
    public class ProductMaster
    {

        public string ProductCode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        public string? Description { get; set; }

        public string? HSNCode { get; set; }

        public decimal GSTPercentage { get; set; }

        public bool IsActive { get; set; }
    }

    public class Product
    {

        // public decimal CostPrice { get; set; }
        // public decimal SellingPrice { get; set; }
        //public int Quantity { get; set; }
    }

    public class ProductVariants
    {
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Material { get; set; }
        public string Gender { get; set; }
        public string Season { get; set; }
        public bool Status { get; set; }
        public ProductVariantPrice ProductVariantPrice { get; set; }=new();    
    }

    public class ProductVariantPrice
    {
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public decimal PlatformFee { get; set; }
        public int Discount { get; set; }
        public string DiscountType { get; set; }
    }


    public class ProductImages
    {
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
    }
}