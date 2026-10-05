# Migrate the WebGL release to Cloudflare

This ExecPlan is a living document.

## Purpose / Big Picture

Publish Gear Engine at `https://leonardolycan.com/games/gear-engine/`, preserve a complete downloadable press kit below Cloudflare's per-file limit, automate preview and production deployments, and retire Firebase Hosting after production acceptance.

## Progress

- [x] Inspect the existing WebGL artifact, press kit, Firebase configuration, Git branches, and domain.
- [x] Add deterministic press-kit optimization, release staging, and static validation.
- [x] Add branch-based Cloudflare deployment automation and operator documentation.
- [x] Generate and validate the optimized press-kit archive.
- [x] Publish and accept the `develop` preview.
- [x] Publish and accept the `main` production route.
- [x] Disable Firebase Hosting and remove active Firebase Hosting configuration.
- [x] Verify the final branches, public URLs, and retirement state.

## Surprises & Discoveries

- The game payload already fits Cloudflare's 25 MiB individual asset limit.
- The original complete press-kit archive is over the limit because it contains full-resolution, 60 fps videos.
- The Unity client has no Firebase SDK dependency; Firebase is only the current static host.

## Decision Log

- Use Cloudflare Workers Static Assets so one Worker can own the game subpath without replacing the rest of the domain.
- Keep `main` as production and `develop` as preview.
- Keep all press-kit categories while optimizing distribution copies of images and videos.
- Disable Firebase Hosting after Cloudflare acceptance while preserving the Firebase project.

## Outcomes & Retrospective

The optimized 20.66 MiB press-kit archive retained every category and passed public download and extraction checks. The Cloudflare preview completed startup, gameplay, results, and rewards. Production version `92990f56-64d4-4da5-bd7f-c310889954d2` served the canonical game, press kit, Brotli WebAssembly, and archive endpoints correctly. GitHub Actions runs `37336885358` (`develop`) and `37337338476` (`main`) both completed validation and deployment successfully from migration commit `852fb868806bf3841878ecf18fc4e14f38b2f76a`. Firebase Hosting was disabled on 2026-10-05 and its former URL returned HTTP 404; the Firebase project was preserved.

## Context and Orientation

The committed release lives under `Artifacts/Submission/GORn2026/GearEngineWebGL`. Hosting tools live under `Artifacts/Submission/GORn2026/CloudflareHosting`. CI is defined in `.github/workflows/CloudflareHosting.yml`.

## Plan of Work

Generate the optimized archive, stage the canonical path, validate size and headers, and perform a Wrangler dry run. Configure Cloudflare credentials, deploy `develop`, and exercise the preview. Merge to `main`, deploy the production route, and exercise the canonical URL. Disable the former Firebase Hosting site, remove its active config files, and re-run the final audit.

## Concrete Steps

Run the scripts documented in `Docs/HostingMigration.md`, publish the branch progression, verify public responses and browser flows, then execute the scoped Firebase Hosting disable command for site `gear-engine-gorn-2026`.

## Validation and Acceptance

Acceptance requires passing local validation, a working Cloudflare preview, a working canonical production route, a valid downloadable press kit below 24 MiB, and confirmation that the previous Firebase Hosting URL no longer serves the game.

## Idempotence and Recovery

Press-kit generation and release staging replace their generated outputs atomically or from a clean directory. Cloudflare deployments are versioned and can be rolled back. Firebase is disabled only after Cloudflare acceptance.

## Artifacts and Notes

The release manifest records every deployed file and SHA-256 digest. The press-kit manifest records archive size, digest, and optimization profile.

## Interfaces and Dependencies

The migration uses Python 3, FFmpeg, ImageMagick, Node.js, Wrangler, GitHub Actions, Cloudflare Workers Static Assets, and the Firebase CLI for final Hosting retirement.
