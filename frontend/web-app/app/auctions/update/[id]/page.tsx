import Heading from '@/app/components/Heading'
import React from 'react'
import AuctionForm from '../../AuctionForm'
import { getDetailedViewData } from '@/app/actions/auctionActions'

export default async function Update({params}: {params: {id: string}}) {
  const data = await getDetailedViewData(params.id);

  return (
    <div className='mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg'>
      <Heading title='Actualize su subasta' subtitle='Por favor actualize la información de su auto.' />
      <AuctionForm auction={data} />
    </div>
  )
}
