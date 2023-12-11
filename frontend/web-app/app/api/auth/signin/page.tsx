import EmptyFilter from '@/app/components/EmptyFilter'
import React from 'react'

export default function Page({searchParams}: {searchParams: {callbackUrl: string}}) {
  return (
    <EmptyFilter 
        title='Debes de iniciar sesión para hacer eso'
        subtitle='Haz click aqui para iniciar sesión'
        showLogin
        callbackUrl={searchParams.callbackUrl}
    />
  )
}
