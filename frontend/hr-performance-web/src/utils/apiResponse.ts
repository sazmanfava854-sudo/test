export function readApiSuccess(raw: unknown): boolean {
  if (!raw || typeof raw !== 'object') return false;
  const obj = raw as Record<string, unknown>;
  const success = obj.success ?? obj.Success;
  return success !== false;
}

export function readApiMessage(raw: unknown): string | undefined {
  if (!raw || typeof raw !== 'object') return undefined;
  const obj = raw as Record<string, unknown>;
  return String(obj.message ?? obj.Message ?? '') || undefined;
}

export function unwrapApiData<T>(raw: unknown): T | null {
  if (!raw || typeof raw !== 'object') return null;
  const obj = raw as Record<string, unknown>;
  if (obj.success === false || obj.Success === false) return null;
  const data = obj.data ?? obj.Data;
  return (data ?? raw) as T;
}

export function unwrapPagedItems<T>(raw: unknown): T[] {
  const data = unwrapApiData<{ items?: T[]; Items?: T[] }>(raw);
  if (!data) return [];
  const items = data.items ?? data.Items;
  return Array.isArray(items) ? items : [];
}

export function unwrapListItems<T>(raw: unknown): T[] {
  const data = unwrapApiData<T[]>(raw);
  if (Array.isArray(data)) return data;
  if (Array.isArray(raw)) return raw as T[];
  return [];
}
