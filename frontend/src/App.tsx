// src/App.tsx
import { useEffect, useState } from 'react';
import type { FeatureCollection } from 'geojson';
import { fetchSitesGeoJson } from './api';
import FilterBar from './components/FilterBar';
import MapView from './components/MapView';
import SidePanel from './components/SidePanel';
import type { SiteCategory } from './types';

const EMPTY: FeatureCollection = { type: 'FeatureCollection', features: [] };

export default function App() {
  const [q, setQ] = useState('');
  const [category, setCategory] = useState<SiteCategory | ''>('');
  const [data, setData] = useState<FeatureCollection>(EMPTY);
  const [selected, setSelected] = useState<string | null>(null);

  // debounced fetch; aborts stale requests
  useEffect(() => {
    const ctrl = new AbortController();
    const t = setTimeout(() => {
      fetchSitesGeoJson({ q, category }, ctrl.signal)
        .then(setData)
        .catch(e => e.name !== 'AbortError' && console.error(e));
    }, 250);
    return () => { clearTimeout(t); ctrl.abort(); };
  }, [q, category]);

  return (
    <div className="relative h-full w-full">
      <MapView data={data} onSelect={setSelected} />
      <FilterBar q={q} category={category} onQ={setQ} onCategory={setCategory} count={data.features.length} />
      {selected && <SidePanel slug={selected} onClose={() => setSelected(null)} />}
    </div>
  );
}