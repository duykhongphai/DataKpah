---
name: DataKpah
description: A compact guild route board for inspecting exact Khí Phách Anh Hùng data.
colors:
  route-yellow: "#f0892b"
  route-yellow-strong: "#c4650f"
  soot-ink: "#151618"
  ink-soft: "#43474d"
  muted-text: "#56616e"
  stone-ground: "#e9edf1"
  paper-surface: "#ffffff"
  stone-surface: "#f3f5f7"
  hairline: "#c6cdd5"
  hairline-soft: "#e0e5ea"
  danger: "#a52b25"
  success: "#167042"
  focus-blue: "#175b9c"
typography:
  display:
    fontFamily: "system-ui, -apple-system, 'Segoe UI', sans-serif"
    fontSize: "clamp(24px, 3vw, 36px)"
    fontWeight: 700
    lineHeight: 1.05
    letterSpacing: "-0.035em"
  title:
    fontFamily: "system-ui, -apple-system, 'Segoe UI', sans-serif"
    fontSize: "16px"
    fontWeight: 700
    lineHeight: 1.28
    letterSpacing: "-0.015em"
  body:
    fontFamily: "system-ui, -apple-system, 'Segoe UI', sans-serif"
    fontSize: "15px"
    fontWeight: 400
    lineHeight: 1.5
    letterSpacing: "normal"
  label:
    fontFamily: "system-ui, -apple-system, 'Segoe UI', sans-serif"
    fontSize: "12px"
    fontWeight: 650
    lineHeight: 1.45
    letterSpacing: "normal"
  data:
    fontFamily: "ui-monospace, 'Cascadia Mono', 'SFMono-Regular', Consolas, monospace"
    fontSize: "12px"
    fontWeight: 650
    lineHeight: 1.25
    letterSpacing: "normal"
rounded:
  keycap: "5px"
  compact: "6px"
  control: "8px"
  field: "9px"
  media: "12px"
  container: "14px"
spacing:
  tight: "5px"
  compact: "8px"
  control: "10px"
  grid: "12px"
  content: "14px"
  cluster: "16px"
  section: "24px"
components:
  route-selected:
    backgroundColor: "{colors.soot-ink}"
    textColor: "{colors.paper-surface}"
    typography: "{typography.label}"
    rounded: "0"
    padding: "0 16px"
    height: "55px"
  field-search:
    backgroundColor: "{colors.stone-surface}"
    textColor: "{colors.soot-ink}"
    typography: "{typography.body}"
    rounded: "{rounded.field}"
    padding: "0 44px 0 40px"
    height: "40px"
  button-primary:
    backgroundColor: "{colors.soot-ink}"
    textColor: "{colors.paper-surface}"
    typography: "{typography.label}"
    rounded: "{rounded.control}"
    padding: "0 13px"
    height: "39px"
  card-record:
    backgroundColor: "{colors.paper-surface}"
    textColor: "{colors.soot-ink}"
    rounded: "{rounded.container}"
    padding: "14px"
  chip-id:
    backgroundColor: "{colors.paper-surface}"
    textColor: "{colors.ink-soft}"
    typography: "{typography.data}"
    rounded: "{rounded.compact}"
    padding: "4px 7px"
---

# Design System: DataKpah

## Overview

**Creative North Star: "The Guild Route Board"**

DataKpah feels like a compact route board installed at a guild archive: direct, sturdy, and built for repeated lookup. Saturated orange gives destinations and selection an unmistakable signal; soot-black type, cool stone surroundings, white work surfaces, and hairline rules keep exact data ahead of ornament.

This is an operating interface, not a fantasy landing page or a generic analytics dashboard. Information density is deliberate, the first real records dominate the view, and identifiers remain visibly machine-readable. Pixel game art is contained and respected rather than enlarged into decoration.

**Key Characteristics:**

- A orange destination band acts as the signature wayfinding device.
- Cool stone layers and white paper surfaces organize dense catalog data.
- System sans typography carries prose; monospace carries IDs, codes, counts, and raw values.
- Compact controls, 12–14px containers, and hairline borders create a precise instrument.
- Motion confirms a route or panel transition without delaying lookup.

## Colors

The palette is mostly cool and mineral; ember orange is the scarce navigational voice, while red, green, and blue remain semantic state colors.

### Primary

- **Ember Route Orange:** The full-width route band, current page marker, selected pagination, empty-state code, and selected-card halo. It signals destination or selection, never generic decoration.
- **Deep Route Rust:** Reserved as a stronger orange companion when the route family needs a darker edge or future pressed-state tone.

### Secondary

- **Focus Blue:** The universal keyboard focus outline; it must remain distinct from both orange selection and red/green system status.

### Tertiary

- **Ready Green:** Freshness-ready status only.
- **Failure Red:** Missing or failed freshness status only.

### Neutral

- **Soot Ink:** Primary text, black pictogram fields, selected route tabs, primary actions, and strong borders.
- **Soft Ink:** Secondary emphatic copy, chip text, and restrained link color.
- **Muted Slate:** Descriptions, labels, freshness text, and non-primary metadata.
- **Cool Stone Ground:** The page field behind all working surfaces.
- **Paper Surface:** Cards, controls, drawer, table, and header surfaces.
- **Soft Stone Surface:** Input wells, card footers, tags, media frames, table heads, and drawer actions.
- **Structural Hairline:** Default borders around interactive controls and containers.
- **Soft Hairline:** Internal separators and low-emphasis outlines.

### Named Rules

**The Orange Means Direction Rule.** Use ember orange for route-finding, selected destinations, and explicit state confirmation; do not wash ordinary cards or large content regions in it.

**The Semantic Colors Stay Semantic Rule.** Red, green, and focus blue communicate error, readiness, and keyboard focus respectively; they are not alternative brand accents.

## Typography

**Display Font:** System UI (with Segoe UI and sans-serif fallbacks)  
**Body Font:** System UI (with Segoe UI and sans-serif fallbacks)  
**Label/Mono Font:** UI monospace (with Cascadia Mono, SFMono-Regular, and Consolas fallbacks)

**Character:** The system sans keeps Vietnamese text native, dense, and quickly legible. Monospace creates the archival voice for route codes, identifiers, counts, and structured values without becoming the general body face.

### Hierarchy

- **Display** (700, fluid 24–36px, 1.05): Catalog heading and other one-per-view titles; tight tracking gives short Vietnamese nouns authority.
- **Detail Title** (700, 24px, 1.15): Record title inside the detail board.
- **Section Title** (700, 22px, normal): Empty, error, and loading-result messages.
- **Card Title** (700, 16px, 1.28): Record names; allow wrapping anywhere for malformed or unusually long game strings.
- **Body** (400, 15px, 1.5): Default interface prose; explanatory lines are constrained to about 70 characters.
- **Label** (650, 11–13px): Tags, controls, navigation, and metadata; uppercase is limited to table headings and detail kinds.
- **Data** (650, 12px, 1.25): IDs, route codes, counts, result notes, metrics, and preformatted JSON.

### Named Rules

**The Data Has Its Own Voice Rule.** Use monospace only where a value is copied, counted, coded, or structurally inspected; names and descriptions stay in system sans.

## Layout

The content shell is wide and operational: a maximum width of 1560px with 20px side gutters on large screens. The sticky header stacks a 62px identity/status rail over a minimum 56px route band. The working area begins 18px below the header, followed by a compact control deck, a 24px section break, then the data stage.

Record cards auto-fill columns at a minimum width of 310px with a 12px grid gap. Card internals use 13–14px spacing and deliberately shallow vertical rhythm. Tables share the same outer container language and collapse nonessential columns on narrow screens. The detail view is a persistent 520px side drawer on desktop and becomes a full-width bottom sheet capped at 88dvh on phones.

At 900px and below, hide the duplicate current-route block and nonessential control labels, preserve a horizontally scrollable route strip, and reduce loading placeholders to two columns. At 680px and below, use 12px page gutters, a two-column control deck with search spanning both columns, one record column, a single-column loading state, and the bottom-sheet detail treatment. The layout must remain usable from the 320px CSS minimum and the product requirement of 360px mobile widths.

**The Records First Rule.** Do not insert a marketing hero between the route band, controls, and real records.

**The Horizontal Route Rule.** On constrained widths, preserve every destination in a touch- and keyboard-scrollable strip instead of collapsing routes into an opaque menu.

## Elevation & Depth

Depth is restrained and structural. Resting cards, tables, and the control deck receive a small 0 2px 4px ambient shadow at 8% soot opacity; borders and tonal layers do most of the separation. Hovered record cards lift only 2px and gain a broader 0 8px 22px shadow at 11%. The detail panel alone earns a strong directional -12px 0 36px shadow at 18%, while the transient toast uses 0 8px 24px at 20% black.

### Named Rules

**The Structural Elevation Rule.** Use low shadow for grouping, stronger shadow for a surface that crosses the page plane, and never add decorative glow.

**The Selected Card Rule.** Selection is a 3px orange halo plus an ink border; elevation alone must not carry selected state.

## Shapes

The system uses gently rounded rectangles, not pills. Major work surfaces and record cards use 14px corners. Media wells use 12px, fields and dense actions use 9px, buttons and transient notices use 8px, and compact tags/IDs use 6px. The 5px keycap is the smallest radius. Hairline borders stay visible throughout; square route segments and black code fields keep the rounded system from feeling soft or ornamental. Status dots are the only fully circular elements.

**The No Pills Rule.** Reserve circles for status; labels, actions, filters, and routes use compact rectangular silhouettes.

## Components

### Route Board Navigation

- **Character:** A saturated orange wayfinding band divided into explicit destinations.
- **Shape:** Full-width square band with hairline ink borders; individual destinations are square segments at a minimum 55px height.
- **Default / Hover:** Transparent over orange; hover adds a restrained 30% white wash.
- **Selected:** Soot ink with white text via `aria-pressed="true"`.
- **Responsive:** The desktop current-route block disappears at 900px; routes remain horizontally scrollable with hidden scrollbar and contained overscroll.
- **Motion:** Current route code, name, and stage title use a 280ms split-flap swap with the standard accelerated ease.

### Inputs / Fields

- **Style:** Search is a 40px soft-stone well with a structural hairline and 9px corners; select and raw-JSON controls use paper white with the same height and corner treatment.
- **Focus:** Every interactive element receives a 3px focus-blue outline with a 2px offset through `:focus-visible`.
- **Placeholder:** Use a lighter slate than body metadata; keep the `/` shortcut in a bordered monospace keycap.

### Buttons

- **Primary:** Soot ink, white text, an 8–9px radius, and a minimum 39–42px height depending on context.
- **Secondary:** Paper surface with soot text and a soot or structural border.
- **Icon:** A 38px square black field with an 8px radius and a simple 20px line icon.
- **Pagination:** Paper buttons are 38px tall; the current page switches to orange with an ink border; disabled controls remain visible at 42% opacity.

### Record Cards

- **Shape:** Paper surface, 14px corners, structural hairline, and clipped overflow.
- **Structure:** A 14px content block over a minimum 43px soft-stone footer. Pixel art sits in an 82px soft-stone well with a 12px radius.
- **Hover:** Lift by 2px over 160ms, darken the border, and expand the ambient shadow.
- **Selected:** Ink border plus orange focus halo; mirror selection through `aria-selected`.
- **Content:** Clamp descriptions to two lines, limit visible tags to four, and keep copyable IDs in a distinct monospace chip.

### Chips

- **Metadata Tag:** Soft-stone fill, soft hairline, 6px corners, and 11px semibold sans text.
- **ID Chip:** Paper fill, structural hairline, 6px corners, and 12px semibold monospace. Copyable instances show the copy icon and pointer cursor.

### Tables

- **Style:** A paper container with 14px corners and the resting shadow. The soft-stone header uses compact uppercase labels and 0.045em tracking.
- **Rows:** Hairline separators; hover uses a pale orange row wash to connect back to route semantics.
- **Responsive:** Below 680px retain only ID and name columns.

### Detail Panel

- **Character:** A route-board extension rather than a modal card.
- **Desktop:** Fixed right drawer, up to 520px wide, entering over 220ms with a strong left-cast structural shadow.
- **Mobile:** Fixed bottom sheet, full width, capped at 88dvh, entering vertically.
- **Header / Footer:** Orange header with ink divider; soft-stone action rail with a full-width black copy action.
- **Accessibility:** Move focus to Close on open, restore it to the invoking record on close, expose hidden state through `aria-hidden` plus `inert` so closed controls leave the tab order, and support Escape.

### Loading, Empty, and Feedback States

- **Loading:** Stone skeleton blocks breathe over 1.1s with alternating delay; no false values are shown.
- **Empty / Error:** A centered paper container uses a orange bordered code tile (`0`, `CHỜ`, or `LỖI`) and direct recovery actions.
- **Freshness:** A status dot and live text distinguish checking, ready, and waiting/error states.
- **Toast:** A compact soot notice rises 14px while fading over 180ms and remains noninteractive.
- **Reduced Motion:** Under `prefers-reduced-motion: reduce`, disable smooth scroll and collapse all animation/transition durations to 0.01ms with one iteration.

## Do's and Don'ts

### Do:

- **Do** use orange to make destinations and selected states legible at a glance.
- **Do** keep real records immediately below a compact control deck.
- **Do** pair tonal layering with visible hairline borders so dense data remains structured without heavy shadow.
- **Do** use monospace for IDs, codes, counts, metrics, and raw structured values.
- **Do** preserve keyboard navigation, a 3px focus-blue outline, focus restoration, live status announcements, and reduced-motion behavior.
- **Do** turn the right drawer into a bottom sheet at the mobile breakpoint.

### Don't:

- **Don't** introduce a decorative fantasy hero, parchment textures, ornamental medieval type, or generic dashboard chrome.
- **Don't** use orange as an all-purpose card background or red/green/blue as decorative accents.
- **Don't** round controls into pills or erase the structural hairlines.
- **Don't** smooth or upscale pixel game art beyond its contained media well.
- **Don't** hide missing data or fabricate game values; use the established loading, waiting, empty, and error states.
- **Don't** depend on motion alone to communicate route change, selection, loading, or panel state.
