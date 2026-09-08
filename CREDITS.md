# Terrain and archipelago references

Atlas's terrain implementation is original Coil code. These sources informed
its design; their source code and artwork are not bundled into the app.

- **Jack Edgar — [The Archipelago Generator](https://sites.google.com/view/jackedgar-gamedev/the-archipelago-generator).**
  The documented progression from a common terrain surface to island regions
  and erosion motivated treating geography as a field instead of independently
  stamping radial islands. Atlas uses world-coordinate domain-warped noise and
  code-site constraints; it does not implement Edgar's Worley partitioning or
  thermal erosion simulation.
- **Amit Patel / Red Blob Games — [Making maps with noise functions](https://www.redblobgames.com/maps/terrain-from-noise/)
  and [Island shaping functions](https://www.redblobgames.com/maps/terrain-from-noise/islands.html).**
  References for combining elevation noise with large-scale shaping constraints
  and distinguishing broad composition from fine detail. Atlas does not use the
  circular-chain “Archipelago” preset.
- **Huftier et al. — [Terrain Synthesis and Authoring based on Iso-Contours](https://doi.org/10.1111/cgf.70389), Computer Graphics Forum.**
  Research reference for controlling coastlines and terrain through coarse
  constraints. Atlas does not implement the paper's Eden Growth algorithm or
  reproduce its experimental results. Atlas's territory isolines use marching
  triangles on a smooth union of reserved footprints.
- **NASA / Landsat — [Mergui Archipelago](https://visibleearth.nasa.gov/images/79831/mergui-archipelago).**
  Visual composition reference: varied island sizes, elongated landmasses,
  uneven waterways and clustered islands. Landsat image by Michael Taylor,
  Landsat Project Science Office; caption by Laura Rocchio. The reference image
  appears in the project research pad, not as an in-game texture.
- **NASA Earth Observatory — [Quirimbas Islands](https://science.nasa.gov/earth/earth-observatory/quirimbas-islands-150933/).**
  Visual reference for shared shallow-water regions between islands. Photograph
  ISS066-E-81982, ISS Crew Earth Observations Facility / NASA Johnson Space Center;
  story by Emily Cassidy.

## Atlas-specific constraints

Files retain individual identities, selectable town halls and historical
building reservations. Conservative circular bounds are collision guards, not
coastline definitions. Terrain is sampled into immutable fine/coarse meshes
before presentation; historical seeking does not run terrain generation.
Shared regional noise provides correlated geography, while translucent shelf
influences blend between neighboring islands. This is an art-directed code map,
not a geological simulation.
