// Shared MUI-based components

const {
  Box, Chip, Avatar, Tooltip,
} = window.MaterialUI;

const TEAM_COLORS = {
  red:     '#ef5350',
  blue:    '#42a5f5',
  green:   '#66bb6a',
  yellow:  '#ffca28',
  magenta: '#ab47bc',
  cyan:    '#26c6da',
};

function WeaponIcon({ name, size }) {
  const w = window.WEAPONS[name] || window.WEAPON_DEFAULT;
  const px = size === 'lg' ? 32 : 24;
  return (
    <Tooltip title={name} arrow>
      <Box sx={{
        width: px, height: px, borderRadius: '6px',
        background: w.color,
        display: 'inline-grid', placeItems: 'center',
        fontFamily: 'JetBrains Mono, monospace',
        fontSize: px * 0.32,
        fontWeight: 700,
        color: 'rgba(0,0,0,0.82)',
        flexShrink: 0,
        cursor: 'default',
        userSelect: 'none',
      }}>
        {w.code}
      </Box>
    </Tooltip>
  );
}

function WeaponChip({ name }) {
  const w = window.WEAPONS[name] || window.WEAPON_DEFAULT;
  return (
    <Chip
      label={name}
      size="small"
      avatar={
        <Avatar sx={{
          bgcolor: w.color + ' !important',
          color: 'rgba(0,0,0,0.82) !important',
          fontFamily: 'JetBrains Mono',
          fontSize: '9px !important',
          fontWeight: 700,
          width: '22px !important',
          height: '22px !important',
        }}>
          {w.code}
        </Avatar>
      }
    />
  );
}

function TeamBadge({ playerId, showTeam }) {
  const p = window.PLAYERS[playerId];
  if (!p) return null;
  return (
    <Chip
      size="small"
      avatar={
        <Avatar sx={{
          bgcolor: TEAM_COLORS[p.color] + ' !important',
          width: '14px !important',
          height: '14px !important',
          fontSize: '0 !important',
        }}> </Avatar>
      }
      label={showTeam ? `${p.name} · ${p.team}` : p.name}
      sx={{ fontWeight: 600, fontSize: 12 }}
    />
  );
}

function PlayerAvatar({ playerId, size }) {
  const p = window.PLAYERS[playerId];
  if (!p) return null;
  const sz = size || 34;
  return (
    <Avatar sx={{
      width: sz, height: sz,
      bgcolor: TEAM_COLORS[p.color],
      color: '#000',
      fontWeight: 700,
      fontSize: sz * 0.42,
    }}>
      {p.avatar}
    </Avatar>
  );
}

// SVG worm silhouette — landing page decoration
function WormBobber() {
  return (
    <div className="worm-rig">
      <svg viewBox="0 0 220 60" fill="none">
        <defs>
          <linearGradient id="wg" x1="0" x2="1" y1="0" y2="1">
            <stop offset="0%" stopColor="#66bb6a"/>
            <stop offset="100%" stopColor="#388e3c"/>
          </linearGradient>
        </defs>
        <rect x="20"  y="22" width="32" height="28" rx="14" fill="url(#wg)"/>
        <rect x="56"  y="20" width="32" height="30" rx="15" fill="url(#wg)"/>
        <rect x="92"  y="16" width="34" height="34" rx="16" fill="url(#wg)"/>
        <rect x="130" y="12" width="46" height="38" rx="18" fill="url(#wg)"/>
        <circle cx="158" cy="24" r="4" fill="white"/>
        <circle cx="170" cy="24" r="4" fill="white"/>
        <circle cx="159" cy="25" r="1.8" fill="black"/>
        <circle cx="171" cy="25" r="1.8" fill="black"/>
        <rect x="132" y="14" width="44" height="4" fill="#ef5350"/>
      </svg>
    </div>
  );
}

window.TEAM_COLORS = TEAM_COLORS;
Object.assign(window, { WeaponIcon, WeaponChip, TeamBadge, PlayerAvatar, WormBobber });
