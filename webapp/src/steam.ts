import { type Request } from 'express'

const OPENID_ENDPOINT = 'https://steamcommunity.com/openid/login'
const OPENID_NS = 'http://specs.openid.net/auth/2.0'

export function getBaseUrl(req: Request): string {
  if (process.env.BASE_URL) return process.env.BASE_URL.replace(/\/$/, '')
  const proto = (req.headers['x-forwarded-proto'] as string) || req.protocol
  const host = req.headers['x-forwarded-host'] || req.headers.host
  return `${proto}://${host}`
}

export function steamLoginUrl(returnTo: string, realm: string): string {
  const p = new URLSearchParams({
    'openid.ns': OPENID_NS,
    'openid.mode': 'checkid_setup',
    'openid.return_to': returnTo,
    'openid.realm': realm,
    'openid.identity': 'http://specs.openid.net/auth/2.0/identifier_select',
    'openid.claimed_id': 'http://specs.openid.net/auth/2.0/identifier_select'
  })
  return `${OPENID_ENDPOINT}?${p.toString()}`
}

export async function verifyOpenId(params: URLSearchParams): Promise<boolean> {
  const body = new URLSearchParams()
  for (const [k, v] of params.entries()) body.append(k, v)
  body.set('openid.mode', 'check_authentication')

  const r = await fetch(OPENID_ENDPOINT, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body
  })
  const text = await r.text()
  return /is_valid\s*:\s*true/i.test(text)
}

export function steamIdFromClaimedId(claimedId: string | null | undefined): string | null {
  if (!claimedId) return null
  const m = claimedId.match(/\/openid\/id\/(\d+)/)
  return m ? m[1] : null
}

export async function getPlayerSummary(steamid: string, key: string) {
  const url = new URL('https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/')
  url.searchParams.set('key', key)
  url.searchParams.set('steamids', steamid)
  const r = await fetch(url)
  if (!r.ok) throw new Error(`Steam API error: ${r.status}`)
  const j = await r.json() as any
  return (j?.response?.players?.[0]) ?? null
}

export async function getOwnedGames(steamid: string, key: string) {
  const url = new URL('https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/')
  url.searchParams.set('key', key)
  url.searchParams.set('steamid', steamid)
  url.searchParams.set('include_appinfo', '1')
  url.searchParams.set('include_played_free_games', '1')
  const r = await fetch(url)
  if (!r.ok) throw new Error(`Steam API error: ${r.status}`)
  const j = await r.json() as any
  return j?.response ?? { game_count: 0, games: [] }
}

