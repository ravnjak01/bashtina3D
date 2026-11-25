// src/api.ts
import type { FeatureCollection } from 'geojson';
import type { SiteCategory, SiteDetail } from './types';

const BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5055';

async function get<T>(path: string, signal?: AbortSignal): Promise<T> {
  const res = await fetch(`${BASE}${path}`, { signal });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return res.json();
}

export function fetchSitesGeoJson(p: { q?: string; category?: SiteCategory | '' }, signal?: AbortSignal) {
  const qs = new URLSearchParams();
  if (p.q) qs.set('q', p.q);
  if (p.category) qs.set('category', p.category);
  return get<FeatureCollection>(`/api/sites/geojson?${qs}`, signal);
}

export const fetchSite = (slug: string, signal?: AbortSignal) =>
  get<SiteDetail>(`/api/sites/by-slug/${encodeURIComponent(slug)}`, signal);