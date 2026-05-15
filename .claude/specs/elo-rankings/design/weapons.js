// Weapon catalog: name -> { code, color, shape }
// Simple geometric "icons" — colored rounded square + 3-letter mono code.
window.WEAPONS = {
  "Bazooka":              { code: "BZK", color: "oklch(0.74 0.175 55)" },
  "Uzi":                  { code: "UZI", color: "oklch(0.72 0.04 250)" },
  "Shotgun":              { code: "SHG", color: "oklch(0.68 0.06 250)" },
  "Grenade":              { code: "GRN", color: "oklch(0.74 0.16 145)" },
  "Cluster Bomb":         { code: "CLB", color: "oklch(0.74 0.16 145)" },
  "Banana Bomb":          { code: "BAN", color: "oklch(0.85 0.16 92)" },
  "Holy Hand-Grenade":    { code: "HHG", color: "oklch(0.84 0.16 90)" },
  "Mine":                 { code: "MIN", color: "oklch(0.66 0.22 25)" },
  "Air Strike":           { code: "AIR", color: "oklch(0.74 0.175 55)" },
  "Napalm Strike":        { code: "NAP", color: "oklch(0.66 0.22 25)" },
  "Mad Cow":              { code: "MAD", color: "oklch(0.70 0.20 330)" },
  "Mole Bomb":            { code: "MOL", color: "oklch(0.55 0.10 60)" },
  "Homing Pigeon":        { code: "HOM", color: "oklch(0.78 0.13 200)" },
  "Dragon Ball":          { code: "DRG", color: "oklch(0.85 0.16 92)" },
  "Fire Punch":           { code: "FIR", color: "oklch(0.74 0.175 55)" },
  "Blow Torch":           { code: "BLW", color: "oklch(0.74 0.175 55)" },
  "Ninja Rope":           { code: "NJR", color: "oklch(0.70 0.04 250)" },
  "Girder":               { code: "GIR", color: "oklch(0.65 0.04 250)" },
  "Low Gravity":          { code: "LOW", color: "oklch(0.78 0.13 200)" },
  "Select Worm":          { code: "SEL", color: "oklch(0.95 0.005 250)" },
  "Jet Pack":             { code: "JET", color: "oklch(0.78 0.13 200)" },
  "Sheep":                { code: "SHP", color: "oklch(0.95 0.005 250)" },
  "Super Sheep":          { code: "SSP", color: "oklch(0.95 0.005 250)" },
  "Concrete Donkey":      { code: "DON", color: "oklch(0.55 0.04 250)" },
};
window.WEAPON_DEFAULT = { code: "???", color: "oklch(0.45 0.02 250)" };
