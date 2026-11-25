// src/components/SidePanel.tsx
import { useEffect, useState } from 'react';
import { fetchSite } from '../api';
import { CATEGORY_COLORS } from '../categories';
import type { SiteDetail } from '../types';

export default function SidePanel({ slug, onClose }: { slug: string; onClose: () => void }) {
  const [site, setSite] = useState<SiteDetail | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const ctrl = new AbortController();
    setSite(null); setError(null);
    fetchSite(slug, ctrl.signal).then(setSite).catch(e => e.name !== 'AbortError' && setError(e.message));
    return () => ctrl.abort();
  }, [slug]);

  return (
    <aside className="absolute right-0 top-0 z-10 h-full w-full max-w-sm overflow-y-auto bg-white shadow-xl">
      <button onClick={onClose} className="absolute right-3 top-3 rounded-full bg-black/50 px-2 text-white" aria-label="Close">✕</button>
      {error && <p className="p-4 text-red-600">Failed to load: {error}</p>}
      {!site && !error && <p className="p-4 text-slate-500">Loading…</p>}
      {site && (
        <>
          {site.heroImageUrl && <img src={site.heroImageUrl} alt={site.name} className="h-48 w-full object-cover" />}
          <div className="space-y-3 p-4">
            <span className="inline-block rounded px-2 py-0.5 text-xs font-medium text-white"
                  style={{ background: CATEGORY_COLORS[site.category] }}>{site.category}</span>
            <h1 className="text-2xl font-semibold">{site.name}</h1>
            {site.nameLocal && site.nameLocal !== site.name && <p className="text-slate-500">{site.nameLocal}</p>}
            <dl className="grid grid-cols-2 gap-2 text-sm">
              {site.municipality && <><dt className="text-slate-500">Municipality</dt><dd>{site.municipality}</dd></>}
              {site.period && <><dt className="text-slate-500">Period</dt><dd>{site.period}</dd></>}
              {site.yearBuilt && <><dt className="text-slate-500">Built</dt><dd>{site.yearBuilt}</dd></>}
              {site.isNationalMonument && <><dt className="text-slate-500">Status</dt><dd>National monument</dd></>}
            </dl>
            {site.description && <p className="text-sm leading-relaxed">{site.description}</p>}
            {site.model && <p className="rounded bg-amber-50 p-2 text-sm">3D model available (coming in a later step)</p>}
            {site.media.map(m => (
              <figure key={m.url}>
                <img src={m.url} alt={m.caption ?? site.name} className="w-full rounded" />
                <figcaption className="text-xs text-slate-500">{m.caption} {m.attribution && `· ${m.attribution}`} {m.license && `(${m.license})`}</figcaption>
              </figure>
            ))}
          </div>
        </>
      )}
    </aside>
  );
}