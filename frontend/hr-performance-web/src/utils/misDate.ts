import { format } from 'date-fns-jalali';

/** سال شمسی از تاریخ میلادی (ISO yyyy-MM-dd) */
export function getShamsiYearFromIso(dateIso: string): string {
  if (!dateIso) return '';
  const d = new Date(`${dateIso}T12:00:00`);
  if (Number.isNaN(d.getTime())) return '';
  return format(d, 'yyyy');
}

export function getShamsiYearRangeLabel(fromIso: string, toIso: string): string {
  const fromYear = getShamsiYearFromIso(fromIso);
  const toYear = getShamsiYearFromIso(toIso);
  if (!fromYear && !toYear) return '';
  if (fromYear === toYear) return fromYear;
  return `${fromYear} تا ${toYear}`;
}

export function formatGregorianDate(iso: string): string {
  if (!iso) return '—';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleDateString('en-CA');
}
