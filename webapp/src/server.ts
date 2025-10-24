import express from 'express'
import path from 'node:path'
import { SignJWT, jwtVerify } from 'jose'
import { getBaseUrl, steamIdFromClaimedId, steamLoginUrl, verifyOpenId, getPlayerSummary, getOwnedGames } from './steam'

const app = express()
app.set('trust proxy', true)
app.use(express.urlencoded({ extended: true }))
app.use(express.json())

// Minimal cookie parser (must come before routes)
app.use((req, _res, next) => {
  const header = req.headers.cookie
  const out: Record<string, string> = {}
  if (header) {
    header.split(';').forEach((pair) => {
      const idx = pair.indexOf('=')
      if (idx !== -1) {
        const k = pair.slice(0, idx).trim()
        const v = decodeURIComponent(pair.slice(idx + 1))
        out[k] = v
      }
    })
  }
  ;(req as any).cookies = out
  next()
})

// Simple static front-end
const publicDir = path.join(process.cwd(), 'webapp', 'public')
app.use(express.static(publicDir))

function getSecret(): Uint8Array {
  const s = process.env.SESSION_SECRET || 'change-me-dev-secret'
  return new TextEncoder().encode(s)
}

async function makeSession(steamid: string) {
  const jwt = await new SignJWT({ steamid })
    .setProtectedHeader({ alg: 'HS256' })
    .setIssuedAt()
    .setExpirationTime('7d')
    .sign(getSecret())
  return jwt
}

async function readSession(token: string | undefined): Promise<{ steamid: string } | null> {
  if (!token) return null
  try {
    const { payload } = await jwtVerify(token, getSecret())
    const steamid = (payload as any).steamid as string | undefined
    return steamid ? { steamid } : null
  } catch {
    return null
  }
}

app.get('/auth/steam', (req, res) => {
  const base = getBaseUrl(req)
  const returnTo = `${base}/auth/steam/return`
  const url = steamLoginUrl(returnTo, base)
  res.redirect(url)
})

app.get('/auth/steam/return', async (req, res) => {
  try {
    const params = new URLSearchParams(req.url.split('?')[1] || '')
    const ok = await verifyOpenId(params)
    if (!ok) return res.status(401).send('OpenID verification failed')

    const steamid = steamIdFromClaimedId(params.get('openid.claimed_id'))
    if (!steamid) return res.status(400).send('Missing SteamID')

    const token = await makeSession(steamid)
    const isProd = process.env.NODE_ENV === 'production'
    res.cookie('session', token, {
      httpOnly: true,
      sameSite: 'lax',
      secure: isProd,
      maxAge: 7 * 24 * 60 * 60 * 1000
    } as any)
    res.redirect('/')
  } catch (err) {
    res.status(500).send('Auth error')
  }
})

app.post('/api/logout', (req, res) => {
  res.clearCookie('session')
  res.status(204).end()
})

app.get('/api/me', async (req, res) => {
  const token = (req as any).cookies?.session || req.headers['x-session'] as string | undefined
  const sess = await readSession(token)
  if (!sess) return res.status(401).json({ error: 'unauthorized' })
  const key = process.env.STEAM_API_KEY
  if (!key) return res.status(500).json({ error: 'STEAM_API_KEY not set' })
  const profile = await getPlayerSummary(sess.steamid, key)
  res.json({ steamid: sess.steamid, profile })
})

app.get('/api/me/games', async (req, res) => {
  const token = (req as any).cookies?.session || req.headers['x-session'] as string | undefined
  const sess = await readSession(token)
  if (!sess) return res.status(401).json({ error: 'unauthorized' })
  const key = process.env.STEAM_API_KEY
  if (!key) return res.status(500).json({ error: 'STEAM_API_KEY not set' })
  const games = await getOwnedGames(sess.steamid, key)
  res.json(games)
})

const port = Number(process.env.PORT || 3000)
app.listen(port, () => {
  console.log(`[steampanno-web] listening on :${port}`)
})
