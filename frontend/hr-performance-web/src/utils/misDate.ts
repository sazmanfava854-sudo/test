import { format, parse, subDays } from 'date-fns-jalali';

export interface ShamsiDateParts {
  year: number;
  month: number;
  day: number;
}

export function toShamsiParts(date: Date): ShamsiDateParts {
  return {
    year: Number(format(date, 'yyyy')),
    month: Number(format(date, 'MM')),
    day: Number(format(date, 'dd')),
  };
}

export function fromShamsiParts(parts: ShamsiDateParts): Date {
  return parse(
    `${parts.year}/${String(parts.month).padStart(2, '0')}/${String(parts.day).padStart(2, '0')}`,
    'yyyy/MM/dd',
    new Date(),
  );
}

export function getDefaultShamsiRange(): { from: ShamsiDateParts; to: ShamsiDateParts } {
  const today = new Date();
  const monthAgo = subDays(today, 30);
  return {
    from: toShamsiParts(monthAgo),
    to: toShamsiParts(today),
  };
}

export function formatShamsiParts(parts: ShamsiDateParts): string {
  return `${parts.year}/${String(parts.month).padStart(2, '0')}/${String(parts.day).padStart(2, '0')}`;
}

export function formatShamsiDate(iso: string): string {
  if (!iso) return '—';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return format(d, 'yyyy/MM/dd');
}

export function formatGregorianDate(iso: string): string {
  if (!iso) return '—';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleDateString('en-CA');
}

export function toMisSyncRequestPayload(
  from: ShamsiDateParts,
  to: ShamsiDateParts,
  employeeLimit = 0,
) {
  return {
    shamsiFromYear: from.year,
    shamsiFromMonth: from.month,
    shamsiFromDay: from.day,
    shamsiToYear: to.year,
    shamsiToMonth: to.month,
    shamsiToDay: to.day,
    employeeLimit,
  };
}

export function isShamsiRangeValid(from: ShamsiDateParts, to: ShamsiDateParts): boolean {
  const fromDate = fromShamsiParts(from).getTime();
  const toDate = fromShamsiParts(to).getTime();
  return toDate >= fromDate;
}

export function compareShamsiParts(a: ShamsiDateParts, b: ShamsiDateParts): number {
  return fromShamsiParts(a).getTime() - fromShamsiParts(b).getTime();
}
