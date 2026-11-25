// src/components/FilterBar.tsx
import { CATEGORIES } from '../categories';
import type { SiteCategory } from '../types';

interface Props {
  q: string; category: SiteCategory | '';
  onQ: (v: string) => void; onCategory: (v: SiteCategory | '') => void;
  count: number;
}

export default function FilterBar({ q, category, onQ, onCategory, count }: Props) {
  return (
    <div className="absolute left-3 top-3 z-10 flex w-80 flex-col gap-2 rounded-lg bg-white/95 p-3 shadow-lg">
      <h1 className="text-lg font-semibold">Baština 3D</h1>
      <input value={q} onChange={e => onQ(e.target.value)} placeholder="Search sites…"
             className="rounded border border-slate-300 px-2 py-1.5 text-sm" />
      <select value={category} onChange={e => onCategory(e.target.value as SiteCategory | '')}
              className="rounded border border-slate-300 px-2 py-1.5 text-sm">
        <option value="">All categories</option>
        {CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
      </select>
      <p className="text-xs text-slate-500">{count} sites</p>
    </div>
  );
}