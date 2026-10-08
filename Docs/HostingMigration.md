# Cloudflare Hosting

Gear Engine is published as static WebGL content through Cloudflare Workers Static Assets.

## Public endpoints

- Landing page: `https://leonardolycan.com/games/gear-engine/`
- Playable game: `https://leonardolycan.com/games/gear-engine/play`
- Press-kit download: `https://leonardolycan.com/games/gear-engine/PressKit/GearEnginePublicPressKit.zip`
- Production branch: `main`
- Preview branch: `develop`

The Worker owns only `/games/gear-engine` and `/games/gear-engine/*`. Other paths on the domain remain with their existing Cloudflare origin and routing rules.

## Release workflow

The workflow at `.github/workflows/CloudflareHosting.yml` validates every relevant pull request. A push to `develop` publishes a Cloudflare preview deployment. A push to `main` publishes the production route.

The GitHub repository requires these Actions secrets:

- `CLOUDFLARE_ACCOUNT_ID`
- `CLOUDFLARE_API_TOKEN`

The API token needs permission to edit Workers scripts and Workers routes for the `leonardolycan.com` zone.

From `Artifacts/Submission/GORn2026/CloudflareHosting`, run:

```text
npm ci
npm run test:routing
npm run prepare
npm run validate
npm run dry-run
```

`prepare` stages the landing page at the canonical root, the committed WebGL release under `play/`, and press-kit resources under `PressKit/` and generates Cloudflare header and redirect rules. `validate` checks the 25 MiB asset limit, WebAssembly and Brotli headers, press-kit archive integrity, and retired Firebase references.

The root landing page references media and fonts under `PressKit/` and links all play buttons to `/games/gear-engine/play`. The player retains relative build and StreamingAssets paths under `play/`. Legacy `/PressKit` page URLs redirect to the landing page; media, the brand kit and the ZIP retain their existing resource paths.

## Press-kit optimization

Run `npm run optimize:presskit` after changing source press-kit files. The script keeps every category, converts art to high-quality WebP, transcodes video to 720 by 1280 at 30 fps, and writes a complete ZIP below the 24 MiB safety target. The archive remains downloadable from the public press-kit page.

## Production acceptance

Before retiring the previous host, verify:

1. The landing page and game return HTTP 200 at their canonical URLs (the game may normalize `/play` to `/play/`). The previous `/PressKit/` page redirects to the landing page.
2. Unity `.unityweb` files return Brotli encoding and correct MIME types.
3. The press-kit ZIP downloads, stays below 24 MiB, and extracts without errors.
4. A clean browser session reaches the menu, starts gameplay, and displays the result and reward screens.
5. The layout works in representative desktop and mobile portrait viewports.

## Rollback

Cloudflare retains Worker deployment versions. Roll back by selecting the preceding healthy deployment in the Cloudflare dashboard, or by redeploying the preceding Git commit from `main`. Keep the route scoped to the game path during rollback.

## Firebase retirement

Firebase Hosting is disabled only after Cloudflare production acceptance. The Firebase project itself remains intact so unrelated project services and historical deployment metadata are not deleted. Once disabled, remove active Firebase Hosting configuration from this repository and verify that the former `web.app` URL no longer serves the game.

### Retirement record

- Retired site: `gear-engine-gorn-2026`
- Disabled: 2026-10-05
- Former URL: `https://gear-engine-gorn-2026.web.app/`
- Verification: HTTP 404 after `firebase hosting:disable`
- Replacement production deployment: Cloudflare version `92990f56-64d4-4da5-bd7f-c310889954d2`
- Firebase project: preserved; only Hosting was disabled
