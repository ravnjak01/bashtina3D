import type { SiteCategory } from './types';

export const CATEGORY_COLORS: Record<SiteCategory, string> = {
  Fortress: '#b45309', Bridge: '#0369a1', Mosque: '#047857', Church: '#7c3aed',
  Monastery: '#a21caf', Necropolis: '#475569', Archaeological: '#ca8a04',
  OldTown: '#dc2626', Museum: '#0d9488', Monument: '#db2777', Other: '#6b7280',
};

export const CATEGORIES = Object.keys(CATEGORY_COLORS) as SiteCategory[];