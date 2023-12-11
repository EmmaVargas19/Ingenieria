using ChocarService.Data;
using ChocarService.Entities;
using ChocarService.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChocarService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly ChocarDbContext _dbContext;
    private readonly IHubContext<NotificationHub> _hubContext;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, ChocarDbContext dbContext, IHubContext<NotificationHub> hubContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting check for finished auctions");

        stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        var finishedAuctions = await _dbContext.Auctions
            .Include(x => x.Item)
            .Include(x => x.Bids)
            .Where(x => x.AuctionEnd <= DateTime.UtcNow && x.Status != Status.Finished)
            .ToListAsync(stoppingToken); 

        
        if (finishedAuctions.Count == 0) return;

        _logger.LogInformation($"==> Found {finishedAuctions.Count} auctions that have completed");

        foreach (var auction in finishedAuctions)
        {
            var winningBid = auction.Bids
            .Where(b => b.BidStatus == BidStatus.Accepted)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefault();

            if (winningBid is not null)
            {
                auction.Winner = winningBid.Bidder;
                auction.SoldAmount = winningBid.Amount;
            }

            auction.Status = auction.SoldAmount > auction.ReservePrice
            ? Status.Finished : Status.ReserveNotMet;


            await _hubContext.Clients.All.SendAsync("AuctionFinished", new AuctionFinishedDto
                {
                    ItemSold = winningBid != null,
                    AuctionId = auction.Id.ToString(),
                    Winner = winningBid?.Bidder,
                    Amount = winningBid?.Amount,
                    Seller = auction.Seller
                }, stoppingToken);
        }

        await _dbContext.SaveChangesAsync(stoppingToken);
    }
}
