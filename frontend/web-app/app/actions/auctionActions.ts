'use server'

import { Auction, Bid, PagedResult } from "@/types";
import { getTokenWorkaround } from "./authActions";
import { fetchWrapper } from "@/app/lib/fetchWrapper";
import { FieldValues } from "react-hook-form";
import { revalidatePath } from "next/cache";

export async function getData(query: string): Promise<PagedResult<Auction>> {
    return await fetchWrapper.get(`api/search/${query}`)
}

export async function updateAuctionTest() {
    const data = {
        mileage: Math.floor(Math.random() * 100000) + 1
    }

    return await fetchWrapper.put('auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c', data);
}

export async function createAuction(data: FieldValues) {
    return await fetchWrapper.post('api/auctions', data);
}

export async function getDetailedViewData(id: string): Promise<Auction> {
    return await fetchWrapper.get(`api/auctions/${id}`);
}

export async function updateAuction(data: FieldValues, id: string) {
    const res = await fetchWrapper.put(`api/auctions/${id}`, data);
    revalidatePath(`api/auctions/${id}`);
    return res;
}

export async function deleteAuction(id: string) {
    return await fetchWrapper.del(`api/auctions/${id}`);
}

export async function getBidsForAuction(id: string): Promise<Bid[]> {
    return await fetchWrapper.get(`api/bids/${id}`);
}

export async function placeBidForAuction(auctionId: string, amount: number) {
    return await fetchWrapper.post(`api/bids?auctionId=${auctionId}&amount=${amount}`, {})
}