Duskhold Castle – HD 2D Platformer Gothic Castle Tileset · FREE v1.1
by dotplays · https://dotplays.itch.io

141 sprites: 48 solid terrain tiles (47-blob autotile + 1 fill variant), 17 background-wall tiles (16-mask, non-colliding),
10 platform pieces (one-way planks, ladder, chain, spikes), 40 props/states, 12 item icons,
8 animated FX sheets, 6 parallax layers. No characters (the demo uses a grey stand-in).

FOLDERS
  terrain/    solid_blob47@128/@64 (8 x 6 atlas), bgwall_16@128/@64 (4 x 4, index = mask), bgwall_fill
  pieces/     single tiles + pieces_sheet@128/@64 (atlas in the order of tiles.json "pieces")
  props/      one PNG per object/state, @128 and @64. Anchor = BOTTOM CENTRE of the image
              (chandelier: top centre, cobweb: top left). Every state of a group has the same size + anchor:
              door (locked/open), save (off/on), breakable (0/1/2/3), chest (closed/open), lever (off/on), portcullis (closed/half/open), plate (up/down), pot (intact/cracked/shattered)
              window_glass = mask of the see-through glass of "window" (draw your sky/parallax inside it)
  icons/      12 item icons, 128 and 64 px squares
  fx/         horizontal frame sheets + fx.json (frame size, frames, fps, anchor; glows are additive)
  parallax/   6 layers at 1920x1080 base (+ @half), parallax.json (order, factor 0-1, y, auto-scroll)
  maps/       level1.json: the demo level (tile letters + objects with anchors in tile units)
  tiled/      castle_solid.tsx (wang set "mixed" = 47-blob), castle_bgwall.tsx (wang set "edge"),
              castle_pieces.tsx (one-way / hazard / climb with collision boxes), castle_props.tsx (objects,
              objectalignment=bottom), level1.tmx (the demo level)
  godot/      castle_tileset.tres (@64) and castle_tileset@128.tres: terrain set 0 "Castle stone" (Match Corners
              and Sides), terrain set 1 "Background wall" (Match Sides), pieces atlas with one-way plank collision,
              custom data "kind". Copy the WHOLE pack folder anywhere under res:// (paths are relative to the .tres).
  demo/       playable demo, plain JavaScript + Canvas 2D. Browsers block file:// fetches, so serve the
              pack folder:  python3 -m http.server   then open http://localhost:8000/demo/
  tiles.json  everything above in one manifest: 47-blob bit order, hitboxes (fractions of a tile),
              anchors, states, flame points (flame_px) for torches/candles, FX timing, parallax.

AUTOTILE (solid)
  bits N=1 NE=2 E=4 SE=8 S=16 SW=32 W=64 NW=128; a corner counts only when both sides next to it are solid.
  tiles.json -> terrain.solid.tiles maps every one of the 47 masks to its atlas index.

SPECS
  tile 128 px master, 64 px export · light from the top-left · PNG with real transparency
  Unity: PPU 128 (or 64 for the @64 files), sprite pivot (0.5, 0) for props.

TILED TIPS
  Paint with the Terrain brush along tile edges and corners. A single isolated block has no terrain id in Tiled:
  stamp it from the tileset. "Fill full tiles" (Ctrl) and the bucket in terrain mode grow the shape by one tile.

Tested in Tiled 1.12.2 and Godot 4.7.2 (47/47 blob + 16/16 background-wall cases, collision, one-way planks).
Please tell us if anything does not import cleanly.

LITE version (2 more stone materials, colour-keyed doors, crumbling floors, lift, 75+ props, 36 icons,
underground parallax, Godot sample scene) is coming to the same itch page.
