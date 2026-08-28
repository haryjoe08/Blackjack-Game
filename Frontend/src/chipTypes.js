// Mirrors BlackjackApi.Models.ChipType exactly (name + underlying value).
// Kept as its own module so both BettingPanel and any future chip display
// (e.g. showing a player's Balance dictionary) can reuse the same colors.
export const CHIP_TYPES = [
  { name: 'White', value: 1, bg: '#f5f5f0', fg: '#1a1a1a', ring: '#c9c9c0' },
  { name: 'Red', value: 5, bg: '#b3122a', fg: '#fff', ring: '#7a0c1c' },
  { name: 'Blue', value: 10, bg: '#1d4f8a', fg: '#fff', ring: '#123258' },
  { name: 'Grey', value: 20, bg: '#6b6b6b', fg: '#fff', ring: '#454545' },
  { name: 'Green', value: 25, bg: '#1f7a3d', fg: '#fff', ring: '#134f27' },
  { name: 'Orange', value: 50, bg: '#d9711d', fg: '#1a1a1a', ring: '#a5540f' },
  { name: 'Black', value: 100, bg: '#1a1a1a', fg: '#f5d67b', ring: '#000' },
  { name: 'Pink', value: 250, bg: '#e0559e', fg: '#1a1a1a', ring: '#a83a74' },
  { name: 'Purple', value: 500, bg: '#6a2c91', fg: '#fff', ring: '#451c60' },
  { name: 'Yellow', value: 1000, bg: '#f2c400', fg: '#1a1a1a', ring: '#b89400' },
  { name: 'LightBlue', value: 2000, bg: '#4fc3f7', fg: '#1a1a1a', ring: '#2a93bd' },
  { name: 'Brown', value: 5000, bg: '#6b4226', fg: '#fff', ring: '#402813' },
]

export function formatChipLabel(value) {
  if (value >= 1000) return `${value / 1000}K`
  return String(value)
}
