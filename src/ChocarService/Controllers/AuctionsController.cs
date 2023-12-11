using ChocarService.Data;
using ChocarService.DTOs;
using ChocarService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChocarService.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ChocarService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionRepository _repo;
    private readonly IMapper _mapper;
    private readonly IHubContext<NotificationHub> _hubContext;

    public AuctionsController(IAuctionRepository repo, IMapper mapper, IHubContext<NotificationHub> hubContext)
    {
        _repo = repo;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        return await _repo.GetAuctionsAsync(date);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _repo.GetAuctionByIdAsync(id);

        if (auction == null) return NotFound();

        return auction;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);

        auction.Seller = User.Identity.Name;

        _repo.AddAuction(auction);
         var newAuction = _mapper.Map<AuctionDto>(auction);
         
        var result = await _repo.SaveChangesAsync();
        if (!result) return BadRequest("Could not save changes to the DB");

        await _hubContext.Clients.All.SendAsync("AuctionCreated", newAuction);

        return CreatedAtAction(nameof(GetAuctionById),
            new { auction.Id }, newAuction);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _repo.GetAuctionEntityById(id);

        if (auction == null) return NotFound();

        if (auction.Seller != User.Identity.Name) return Forbid();

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await _repo.SaveChangesAsync();

        if (result) return Ok();

        return BadRequest("Problem updating auction");
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _repo.GetAuctionEntityById(id);

        if (auction == null) return NotFound();

        if (auction.Seller != User.Identity.Name) return Forbid();

        _repo.RemoveAuction(auction);

        var result = await _repo.SaveChangesAsync();

        if (!result) return BadRequest("Problem deleting auction");

        return Ok();
    }
}
