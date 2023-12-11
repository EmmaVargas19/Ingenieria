using AutoMapper;
using ChocarService;
using ChocarService.Data;
using ChocarService.DTOs;
using ChocarService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace SearchService;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{

    private readonly ChocarDbContext _repo;
    private readonly IMapper _mapper;

    public SearchController(ChocarDbContext repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = _repo.Auctions.Include(x => x.Item) as IQueryable<Auction>;

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Where(x => x.Item.Make.Contains(searchParams.SearchTerm)
                          || x.Item.Model.Contains(searchParams.SearchTerm)
                          || x.Item.Color.Contains(searchParams.SearchTerm)
                          || x.Winner.Contains(searchParams.SearchTerm)
                          || x.Seller.Contains(searchParams.SearchTerm));
        }

        query = searchParams.OrderBy switch
        {   
            "make" => query.OrderBy(x => x.Item.Make)
                        .ThenBy(x => x.Item.Model),
            "new" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.AuctionEnd)
        };

        query = searchParams.FilterBy switch
        {
            "finished" => query.Where(x => x.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Where(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6)
                && x.AuctionEnd > DateTime.UtcNow),
            _ => query.Where(x => x.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Where(x => x.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Where(x => x.Winner == searchParams.Winner);
        }

        var result = await query
        .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
        .Take(searchParams.PageSize)
        .ToListAsync();

        return Ok(new
        {
            results = _mapper.Map<List<AuctionDto>>(result),
            pageCount = (int)Math.Ceiling(await query.CountAsync() / (double)searchParams.PageSize),
            totalCount = await query.CountAsync()
        });
    }
}
