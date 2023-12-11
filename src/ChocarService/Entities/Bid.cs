
namespace ChocarService.Entities;

public class Bid
{
    public Guid Id { get; set; }
    public string Bidder { get; set; }
    public DateTime BidTime { get; set; } = DateTime.UtcNow;
    public int Amount { get; set; }
    public BidStatus BidStatus { get; set; }

    // navigation
    public Auction Auction { get; set; }
    public Guid AuctionId { get; set; }
}
