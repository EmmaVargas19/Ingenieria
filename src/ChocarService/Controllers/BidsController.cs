using AutoMapper;
using ChocarService.Data;
using ChocarService.DTOs;
using ChocarService.Entities;
using ChocarService.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BiddingService;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly ChocarDbContext _dbcontext;
    private readonly IMapper _mapper;
    private readonly IHubContext<NotificationHub> _hubContext;

    public BidsController(ChocarDbContext dbcontext, IMapper mapper, IHubContext<NotificationHub> hubContext)
    {
        _dbcontext = dbcontext;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        var auction = await _dbcontext.Auctions
            .Include(x => x.Bids)
            .FirstOrDefaultAsync(x => x.Id == Guid.Parse(auctionId));

        if (auction == null) return BadRequest("Cannot place bids on this auction");


        if (auction.Seller == User.Identity.Name)
        {
            return BadRequest("You cannot bid on your own auction");
        }

        var bid = new Bid
        {
            Amount = amount,
            AuctionId = Guid.Parse(auctionId),
            Bidder = User.Identity.Name,
            Auction = auction,
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highestBidSoFar = auction.Bids.OrderByDescending(x => x.Amount).FirstOrDefault();

            if (highestBidSoFar != null && amount > highestBidSoFar.Amount || highestBidSoFar == null)
            {
                bid.BidStatus = amount > auction.ReservePrice
                    ? BidStatus.Accepted
                    : BidStatus.AcceptedBelowReserve;
            }

            if (highestBidSoFar != null && bid.Amount <= highestBidSoFar.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }

        if (auction.CurrentHighBid == null || bid.BidStatus == BidStatus.Accepted && bid.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = bid.Amount;
        }

        _dbcontext.Bids.Add(bid);
        await _dbcontext.SaveChangesAsync();

        var bidDto = _mapper.Map<BidDto>(bid);
        await _hubContext.Clients.All.SendAsync("BidPlaced", bidDto);

        return Ok(bidDto);
    }

    [HttpGet("{auctionId}")]
    public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
    {
        var bids = await _dbcontext.Bids
            .Where(b => b.AuctionId.ToString() == auctionId)
            .OrderByDescending(b => b.BidTime)
            .ToListAsync();

        return _mapper.Map<List<BidDto>>(bids);
        
    }
}
