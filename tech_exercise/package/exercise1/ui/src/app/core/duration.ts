/**
 * How long a period lasted, in whole years and months, e.g. "2 years, 5 months". `end` is exclusive,
 * so a duty ending on 30 June is passed 1 July.
 */
export function describeDuration(start: Date, end: Date): string {
  let months = (end.getFullYear() - start.getFullYear()) * 12 + (end.getMonth() - start.getMonth());
  if (end.getDate() < start.getDate()) {
    months--; // The last month isn't complete yet.
  }

  if (months < 1) {
    return 'less than a month';
  }

  const years = Math.floor(months / 12);
  const remainder = months % 12;
  return [plural(years, 'year'), plural(remainder, 'month')].filter(Boolean).join(', ');
}

function plural(count: number, unit: string): string {
  return count === 0 ? '' : `${count} ${unit}${count === 1 ? '' : 's'}`;
}

/** The day after an ISO date, for turning an inclusive end date into an exclusive one. */
export function dayAfter(isoDate: string): Date {
  const date = new Date(isoDate);
  date.setDate(date.getDate() + 1);
  return date;
}
