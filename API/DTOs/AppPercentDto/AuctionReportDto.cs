namespace API.DTOs
{
    public class AuctionReportDto
    {
        public string Client { get; set; }
        public string CarIdNumber { get; set; }
        public double SellerAmount { get; set; }
        public double Insurance { get; set; }
        public double AppPercent { get; set; }
        public double Total { get; set; }
        public Double CarReportCommission { get; set; }

    }
}