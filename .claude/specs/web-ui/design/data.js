// ── Players ───────────────────────────────────────────────────────────────
window.PLAYERS = {
  eadie:   { id: "eadie",   name: "Eadie",   team: "Eadie's Army",    color: "red",     avatar: "E", grad: "linear-gradient(135deg, #ef5350, #b71c1c)" },
  skip:    { id: "skip",    name: "Skip",    team: "Phunny",          color: "blue",    avatar: "S", grad: "linear-gradient(135deg, #42a5f5, #1565c0)" },
  jezza:   { id: "jezza",  name: "Jezza32", team: "Guards! Guards!",  color: "green",   avatar: "J", grad: "linear-gradient(135deg, #66bb6a, #2e7d32)" },
  voxel:   { id: "voxel",  name: "Voxel",   team: "Holy Mole",       color: "yellow",  avatar: "V", grad: "linear-gradient(135deg, #ffca28, #f57f17)" },
  krang:   { id: "krang",  name: "Krang",   team: "Wormy Wonders",   color: "magenta", avatar: "K", grad: "linear-gradient(135deg, #ab47bc, #6a1b9a)" },
  brick:   { id: "brick",  name: "Brick",   team: "Dynamite Inc",    color: "cyan",    avatar: "B", grad: "linear-gradient(135deg, #26c6da, #00838f)" },
  phantom: { id: "phantom",name: "Phantom", team: "Ghost Squad",     color: "magenta", avatar: "P", grad: "linear-gradient(135deg, #ce93d8, #7b1fa2)" },
  nova:    { id: "nova",   name: "Nova",    team: "Stellar Worms",   color: "cyan",    avatar: "N", grad: "linear-gradient(135deg, #80deea, #006064)" },
};

// ── Leagues ───────────────────────────────────────────────────────────────
window.LEAGUE_DEFS = {
  bbl: {
    id: "bbl", name: "Banana Bomb League", short: "BBL",
    season: "Season 4", week: "Week 6 of 12",
    color: "#1976d2", description: "The original Friday-night FFA. First to 10 season wins takes the cup.",
    playerIds: ["eadie","skip","jezza","voxel","krang","brick"],
  },
  hhl: {
    id: "hhl", name: "Holy Hand League", short: "HHL",
    season: "Season 2", week: "Week 2 of 8",
    color: "#66bb6a", description: "Holy Hand-Grenades only? No — but heavily encouraged. High chaos, low mercy.",
    playerIds: ["skip","jezza","voxel","phantom"],
  },
  smw: {
    id: "smw", name: "Super Mega Worms", short: "SMW",
    season: "Season 1", week: "Week 8 · Final",
    color: "#ab47bc", description: "Season 1 — the inaugural league. Currently in the grand final week.",
    playerIds: ["eadie","brick","krang","nova"],
  },
};

// ── Featured game turns (BBL #0042 from API example) ─────────────────────
const FEATURED_TURNS = [
  { num: 1,  team: "Eadie's Army",    weapons: ["Bazooka"], damage: { "Phunny": 23, "Guards! Guards!": 9 } },
  { num: 2,  team: "Phunny",          weapons: ["Uzi"], damage: { "Eadie's Army": 13, "Phunny": 3, "Guards! Guards!": 4 } },
  { num: 3,  team: "Guards! Guards!", weapons: ["Low Gravity","Fire Punch"], damage: { "Eadie's Army": 100 }, kills: { "Eadie's Army": 1 } },
  { num: 4,  team: "Eadie's Army",    weapons: ["Select Worm","Dragon Ball"], damage: { "Guards! Guards!": 49 }, kills: { "Guards! Guards!": 1 } },
  { num: 5,  team: "Phunny",          weapons: ["Ninja Rope","Ninja Rope","Girder"], damage: {} },
  { num: 6,  team: "Guards! Guards!", weapons: ["Select Worm","Ninja Rope","Ninja Rope","Ninja Rope","Grenade"], damage: { "Phunny": 27 } },
  { num: 7,  team: "Eadie's Army",    weapons: ["Girder"], damage: {} },
  { num: 8,  team: "Phunny",          weapons: ["Shotgun","Shotgun"], damage: { "Guards! Guards!": 45 } },
  { num: 9,  team: "Guards! Guards!", weapons: ["Ninja Rope","Bazooka"], damage: { "Phunny": 33 } },
  { num: 10, team: "Eadie's Army",    weapons: ["Ninja Rope","Mine"], damage: { "Eadie's Army": 3, "Phunny": 40 }, kills: { "Phunny": 1 } },
  { num: 11, team: "Phunny",          weapons: ["Grenade"], damage: {} },
  { num: 12, team: "Guards! Guards!", weapons: ["Ninja Rope","Ninja Rope"], damage: {} },
  { num: 13, team: "Eadie's Army",    weapons: ["Ninja Rope","Holy Hand-Grenade"], damage: { "Eadie's Army": 5, "Phunny": 77 }, kills: { "Phunny": 1 } },
  { num: 14, team: "Phunny",          weapons: ["Holy Hand-Grenade"], damage: { "Eadie's Army": 37 } },
  { num: 15, team: "Guards! Guards!", weapons: ["Air Strike"], damage: { "Phunny": 42 } },
  { num: 16, team: "Eadie's Army",    weapons: ["Napalm Strike"], damage: { "Phunny": 55, "Guards! Guards!": 22 }, kills: { "Phunny": 1 } },
  { num: 17, team: "Phunny",          weapons: ["Mad Cow"], damage: { "Guards! Guards!": 77 }, kills: { "Guards! Guards!": 1 } },
  { num: 18, team: "Guards! Guards!", weapons: ["Napalm Strike"], damage: { "Eadie's Army": 15 } },
  { num: 19, team: "Eadie's Army",    weapons: ["Ninja Rope","Ninja Rope","Banana Bomb"], damage: { "Phunny": 100 }, kills: { "Phunny": 1 } },
  { num: 20, team: "Guards! Guards!", weapons: ["Girder"], damage: {} },
  { num: 21, team: "Eadie's Army",    weapons: ["Air Strike"], damage: { "Guards! Guards!": 13 } },
  { num: 22, team: "Guards! Guards!", weapons: ["Holy Hand-Grenade"], damage: { "Guards! Guards!": 14 } },
  { num: 23, team: "Eadie's Army",    weapons: ["Blow Torch"], damage: {} },
  { num: 24, team: "Guards! Guards!", weapons: ["Ninja Rope","Ninja Rope","Mole Bomb"], damage: {} },
  { num: 25, team: "Eadie's Army",    weapons: ["Homing Pigeon"], damage: { "Guards! Guards!": 28 }, kills: { "Guards! Guards!": 1 } },
];

// ── All games ─────────────────────────────────────────────────────────────
window.GAMES = [
  // ── BBL ──────────────────────────────────────────────────────────────
  {
    id: "g-0042", leagueId: "bbl", when: "2026-05-01T11:29:28", duration: 1684,
    map: "Cavern", players: ["eadie","skip","jezza"], winner: "eadie",
    finals: { eadie: 187, skip: 0, jezza: 0 },
    turns: FEATURED_TURNS, featured: true, replaySize: "412 KB",
  },
  { id: "g-0041", leagueId: "bbl", when: "2026-04-29T20:14:02", duration: 1242, map: "Hell",
    players: ["voxel","skip","krang"], winner: "voxel", finals: { voxel: 142, skip: 0, krang: 0 },
    topWeapons: ["Holy Hand-Grenade","Banana Bomb","Ninja Rope"], totalDamage: 798, turnCount: 22 },
  { id: "g-0040", leagueId: "bbl", when: "2026-04-28T19:02:11", duration: 1820, map: "Forest",
    players: ["jezza","brick","eadie"], winner: "jezza", finals: { jezza: 96, brick: 0, eadie: 0 },
    topWeapons: ["Bazooka","Grenade","Air Strike"], totalDamage: 912, turnCount: 31 },
  { id: "g-0039", leagueId: "bbl", when: "2026-04-27T22:48:55", duration: 988, map: "Desert",
    players: ["krang","brick","skip"], winner: "krang", finals: { krang: 124, brick: 0, skip: 0 },
    topWeapons: ["Shotgun","Uzi","Mine"], totalDamage: 632, turnCount: 18 },
  { id: "g-0038", leagueId: "bbl", when: "2026-04-26T18:30:00", duration: 2102, map: "Cavern",
    players: ["eadie","voxel","jezza"], winner: "eadie", finals: { eadie: 211, voxel: 0, jezza: 0 },
    topWeapons: ["Holy Hand-Grenade","Bazooka","Banana Bomb"], totalDamage: 1104, turnCount: 36 },
  { id: "g-0037", leagueId: "bbl", when: "2026-04-25T21:11:43", duration: 1456, map: "Hospital",
    players: ["skip","brick","krang"], winner: "skip", finals: { skip: 168, brick: 0, krang: 0 },
    topWeapons: ["Grenade","Cluster Bomb","Ninja Rope"], totalDamage: 844, turnCount: 26 },
  { id: "g-0036", leagueId: "bbl", when: "2026-04-24T20:00:08", duration: 1320, map: "Construction",
    players: ["jezza","eadie","voxel"], winner: "jezza", finals: { jezza: 102, eadie: 0, voxel: 0 },
    topWeapons: ["Air Strike","Napalm Strike","Mad Cow"], totalDamage: 766, turnCount: 24 },
  { id: "g-0035", leagueId: "bbl", when: "2026-04-23T19:18:29", duration: 1592, map: "Cheese",
    players: ["brick","krang","skip"], winner: "brick", finals: { brick: 158, krang: 0, skip: 0 },
    topWeapons: ["Homing Pigeon","Bazooka","Holy Hand-Grenade"], totalDamage: 880, turnCount: 28 },
  { id: "g-0034", leagueId: "bbl", when: "2026-04-22T20:42:17", duration: 1100, map: "Pirate",
    players: ["voxel","eadie","jezza"], winner: "voxel", finals: { voxel: 88, eadie: 0, jezza: 0 },
    topWeapons: ["Banana Bomb","Shotgun","Dragon Ball"], totalDamage: 720, turnCount: 20 },
  { id: "g-0033", leagueId: "bbl", when: "2026-04-21T18:55:01", duration: 1834, map: "Tribal",
    players: ["eadie","brick","krang"], winner: "eadie", finals: { eadie: 134, brick: 0, krang: 0 },
    topWeapons: ["Bazooka","Mine","Ninja Rope"], totalDamage: 956, turnCount: 32 },
  { id: "g-0032", leagueId: "bbl", when: "2026-04-20T19:33:48", duration: 980, map: "Easter",
    players: ["skip","jezza","voxel"], winner: "skip", finals: { skip: 76, jezza: 0, voxel: 0 },
    topWeapons: ["Uzi","Grenade","Fire Punch"], totalDamage: 588, turnCount: 17 },
  { id: "g-0031", leagueId: "bbl", when: "2026-04-19T21:07:22", duration: 1612, map: "Manhattan",
    players: ["jezza","krang","brick"], winner: "jezza", finals: { jezza: 118, krang: 0, brick: 0 },
    topWeapons: ["Holy Hand-Grenade","Banana Bomb","Air Strike"], totalDamage: 902, turnCount: 28 },

  // ── HHL ──────────────────────────────────────────────────────────────
  { id: "g-0119", leagueId: "hhl", when: "2026-05-03T20:00:00", duration: 1102, map: "Hell",
    players: ["skip","phantom","jezza"], winner: "phantom", finals: { phantom: 144, skip: 0, jezza: 0 },
    topWeapons: ["Holy Hand-Grenade","Banana Bomb","Ninja Rope"], totalDamage: 768, turnCount: 20 },
  { id: "g-0118", leagueId: "hhl", when: "2026-05-01T19:45:00", duration: 1388, map: "Forest",
    players: ["voxel","skip","phantom"], winner: "skip", finals: { skip: 112, voxel: 0, phantom: 0 },
    topWeapons: ["Holy Hand-Grenade","Bazooka","Air Strike"], totalDamage: 692, turnCount: 22 },
  { id: "g-0117", leagueId: "hhl", when: "2026-04-29T21:00:00", duration: 1560, map: "Cavern",
    players: ["jezza","phantom","voxel"], winner: "jezza", finals: { jezza: 88, phantom: 0, voxel: 0 },
    topWeapons: ["Holy Hand-Grenade","Shotgun","Ninja Rope"], totalDamage: 834, turnCount: 26 },
  { id: "g-0116", leagueId: "hhl", when: "2026-04-27T20:30:00", duration: 924, map: "Desert",
    players: ["skip","jezza","phantom"], winner: "skip", finals: { skip: 198, jezza: 0, phantom: 0 },
    topWeapons: ["Holy Hand-Grenade","Banana Bomb","Grenade"], totalDamage: 922, turnCount: 18 },
  { id: "g-0115", leagueId: "hhl", when: "2026-04-25T19:15:00", duration: 1244, map: "Hospital",
    players: ["phantom","voxel","jezza"], winner: "voxel", finals: { voxel: 122, phantom: 0, jezza: 0 },
    topWeapons: ["Holy Hand-Grenade","Homing Pigeon","Uzi"], totalDamage: 744, turnCount: 21 },
  { id: "g-0114", leagueId: "hhl", when: "2026-04-23T21:00:00", duration: 1688, map: "Cheese",
    players: ["skip","voxel","phantom"], winner: "phantom", finals: { phantom: 104, skip: 0, voxel: 0 },
    topWeapons: ["Holy Hand-Grenade","Napalm Strike","Ninja Rope"], totalDamage: 788, turnCount: 28 },
  { id: "g-0113", leagueId: "hhl", when: "2026-04-21T20:00:00", duration: 1320, map: "Pirate",
    players: ["jezza","skip","voxel"], winner: "jezza", finals: { jezza: 176, skip: 0, voxel: 0 },
    topWeapons: ["Holy Hand-Grenade","Mad Cow","Air Strike"], totalDamage: 856, turnCount: 24 },
  { id: "g-0112", leagueId: "hhl", when: "2026-04-19T19:30:00", duration: 998, map: "Construction",
    players: ["phantom","jezza","skip"], winner: "jezza", finals: { jezza: 92, phantom: 0, skip: 0 },
    topWeapons: ["Holy Hand-Grenade","Banana Bomb","Mine"], totalDamage: 612, turnCount: 16 },

  // ── SMW ──────────────────────────────────────────────────────────────
  { id: "g-0208", leagueId: "smw", when: "2026-05-04T17:00:00", duration: 2240, map: "Tribal",
    players: ["eadie","nova","krang"], winner: "nova", finals: { nova: 214, eadie: 0, krang: 0 },
    topWeapons: ["Banana Bomb","Concrete Donkey","Napalm Strike"], totalDamage: 1244, turnCount: 38 },
  { id: "g-0207", leagueId: "smw", when: "2026-05-02T18:30:00", duration: 1904, map: "Manhattan",
    players: ["brick","krang","eadie"], winner: "brick", finals: { brick: 188, krang: 0, eadie: 0 },
    topWeapons: ["Banana Bomb","Holy Hand-Grenade","Air Strike"], totalDamage: 1088, turnCount: 33 },
  { id: "g-0206", leagueId: "smw", when: "2026-04-30T19:00:00", duration: 1648, map: "Easter",
    players: ["nova","eadie","brick"], winner: "eadie", finals: { eadie: 166, nova: 0, brick: 0 },
    topWeapons: ["Bazooka","Banana Bomb","Ninja Rope"], totalDamage: 944, turnCount: 29 },
  { id: "g-0205", leagueId: "smw", when: "2026-04-28T20:00:00", duration: 1432, map: "Pirate",
    players: ["krang","nova","brick"], winner: "krang", finals: { krang: 132, nova: 0, brick: 0 },
    topWeapons: ["Banana Bomb","Dragon Ball","Mad Cow"], totalDamage: 876, turnCount: 25 },
  { id: "g-0204", leagueId: "smw", when: "2026-04-26T17:30:00", duration: 1780, map: "Forest",
    players: ["eadie","brick","nova"], winner: "nova", finals: { nova: 158, eadie: 0, brick: 0 },
    topWeapons: ["Banana Bomb","Grenade","Ninja Rope"], totalDamage: 1012, turnCount: 31 },
  { id: "g-0203", leagueId: "smw", when: "2026-04-24T19:45:00", duration: 1256, map: "Cheese",
    players: ["brick","eadie","krang"], winner: "eadie", finals: { eadie: 148, brick: 0, krang: 0 },
    topWeapons: ["Banana Bomb","Bazooka","Homing Pigeon"], totalDamage: 818, turnCount: 22 },
  { id: "g-0202", leagueId: "smw", when: "2026-04-22T18:00:00", duration: 2080, map: "Hell",
    players: ["nova","krang","eadie"], winner: "nova", finals: { nova: 196, krang: 0, eadie: 0 },
    topWeapons: ["Banana Bomb","Napalm Strike","Concrete Donkey"], totalDamage: 1168, turnCount: 35 },
  { id: "g-0201", leagueId: "smw", when: "2026-04-20T20:15:00", duration: 1544, map: "Construction",
    players: ["krang","brick","nova"], winner: "krang", finals: { krang: 162, brick: 0, nova: 0 },
    topWeapons: ["Banana Bomb","Mine","Shotgun"], totalDamage: 892, turnCount: 27 },
];

// ── Helpers ───────────────────────────────────────────────────────────────
window.fmtDate = (iso) => new Date(iso).toLocaleDateString("en-GB", { day: "2-digit", month: "short" });
window.fmtTime = (iso) => new Date(iso).toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
window.fmtRelative = (iso) => {
  const days = Math.floor((Date.now() - new Date(iso)) / 86400000);
  if (days === 0) return "Today"; if (days === 1) return "Yesterday";
  if (days < 7) return `${days}d ago`; return `${Math.floor(days/7)}w ago`;
};
window.fmtDuration = (s) => `${Math.floor(s/60)}:${String(s%60).padStart(2,"0")}`;

window.gamesForLeague = (leagueId) => window.GAMES.filter(g => g.leagueId === leagueId);

window.summarizeFeatured = () => {
  const g = window.GAMES.find(g => g.featured);
  const damage = {}, weaponCount = {}; let totalDamage = 0;
  g.turns.forEach(t => {
    t.weapons.forEach(w => { weaponCount[w] = (weaponCount[w] || 0) + 1; });
    Object.entries(t.damage || {}).forEach(([, d]) => { totalDamage += d; });
  });
  const topWeapons = Object.entries(weaponCount)
    .filter(([w]) => !["Ninja Rope","Girder","Select Worm"].includes(w))
    .sort((a,b) => b[1]-a[1]).slice(0,6);
  return { weaponCount, topWeapons, totalDamage, turnCount: g.turns.length };
};
