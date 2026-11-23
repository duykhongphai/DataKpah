# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

Delegated from the user's request: static HTML, CSS, and vanilla JavaScript deployed directly
from the repository root on GitHub Pages. The data collector remains a .NET 10 command-line
application run by GitHub Actions.

## Users

Vietnamese Khí Phách Anh Hùng players and developers who need to inspect game catalogs quickly on
desktop or mobile, without opening the game client or reading raw packet dumps.

## Product Purpose

DataKpah automatically extracts the data the official KPAH client receives, keeps a shared daily
dataset in the repository, and presents that dataset as a searchable web reference.

## Positioning

Unlike a hand-maintained wiki, DataKpah is generated from the live game protocol and exposes the
source IDs, images, attributes, levels, and raw JSON that the client itself uses.

## Operating Context

The GitHub Action logs into several configured characters in sequence, merges their data into
`TeaMobi/DuLieu`, and commits changes. Visitors use category navigation, search, sorting, and raw
JSON links to inspect the result. The site must remain useful while data is loading, absent, or
partially available during the first collection run.

## Capabilities and Constraints

- Vietnamese interface with equipment, potion, gem, shop item, monster (animated sprite), and skill
  (grouped by the five classes) views where corresponding JSON exists.
- Static GitHub Pages hosting; no server-side runtime and no frontend build step.
- Images and JSON are repository-relative and may arrive after the site code is deployed.
- Game data is shared across servers; configured characters are collection sources, not separate
  public datasets.
- Never invent missing game values. Empty, unavailable, and malformed data need explicit states.

## Evidence on Hand

- The live protocol implementation is in `HeadlessClient/DataKpah.HeadlessClient/`.
- The scheduled collector is `.github/workflows/update-data.yml`.
- Data schemas are represented by the C# records in `DuLieu.cs` and filenames in `BoXuat.cs`.
- No existing KPAH visual identity, logo asset, testimonials, or performance claims were supplied.

## Product Principles

- Put exact game data ahead of decoration.
- Make IDs and source JSON easy to inspect and copy.
- Preserve useful partial results instead of hiding the entire site on one failed dataset.
- Work equally well for fast lookup on a phone and broad comparison on a desktop.
- Make freshness and collection state visible and honest.

## Accessibility & Inclusion

Keyboard navigation, visible focus, semantic controls, reduced-motion support, sufficient color
contrast, and layouts usable from 360px mobile widths are required.
