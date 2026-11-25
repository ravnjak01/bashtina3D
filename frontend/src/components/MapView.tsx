// src/components/MapView.tsx
import { useEffect, useRef } from 'react';
import * as maplibregl from 'maplibre-gl';
import type { ExpressionSpecification, GeoJSONSource } from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import mapLibreWorkerUrl from 'maplibre-gl/dist/maplibre-gl-worker.mjs?url';
import type { FeatureCollection, Point } from 'geojson';
import { CATEGORY_COLORS } from '../categories';

maplibregl.setWorkerUrl(mapLibreWorkerUrl);

const EMPTY: FeatureCollection = { type: 'FeatureCollection', features: [] };
const colorExpr = [
  'match', ['get', 'category'],
  ...Object.entries(CATEGORY_COLORS).flat(),
  '#6b7280',
] as unknown as ExpressionSpecification;

interface Props { data: FeatureCollection; onSelect: (slug: string) => void }

export default function MapView({ data, onSelect }: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const mapRef = useRef<maplibregl.Map | null>(null);
  const readyRef = useRef(false);
  const dataRef = useRef(data);
  const onSelectRef = useRef(onSelect);
  dataRef.current = data;
  onSelectRef.current = onSelect;

  // init once
  useEffect(() => {
    const map = new maplibregl.Map({
      container: containerRef.current!,
      style: 'https://tiles.openfreemap.org/styles/liberty',   // 
      center: [17.8, 44.0],                                      // [lon, lat]
      zoom: 7, pitch: 55, bearing: -10, maxPitch: 80,
    });
    mapRef.current = map;
    map.addControl(new maplibregl.NavigationControl({ visualizePitch: true }), 'bottom-right');

    const demTiles = ['https://s3.amazonaws.com/elevation-tiles-prod/terrarium/{z}/{x}/{y}.png'];
const demSource = {
  type: 'raster-dem' as const, encoding: 'terrarium' as const, tileSize: 256, maxzoom: 15,
  tiles: demTiles, attribution: 'Terrain: Mapzen / AWS Open Data',
};

    map.on('load', () => {
      // --- terrain (AWS open Terrarium tiles, no key) ---
      map.addSource('dem', demSource);
      map.addSource('dem-hillshade', demSource);
      const firstSymbol = map.getStyle().layers.find(l => l.type === 'symbol')?.id;  // keep labels on top
      map.addLayer({ id: 'hillshade', type: 'hillshade', source: 'dem-hillshade',
        paint: { 'hillshade-exaggeration': 0.45 } }, firstSymbol);
      map.setTerrain({ source: 'dem', exaggeration: 1.3 });

      // --- sites, clustered ---
      map.addSource('sites', { type: 'geojson', data: dataRef.current, cluster: true, clusterRadius: 50, clusterMaxZoom: 12 });

      map.addLayer({ id: 'clusters', type: 'circle', source: 'sites', filter: ['has', 'point_count'],
        paint: {
          'circle-color': '#1e293b', 'circle-opacity': 0.85,
          'circle-radius': ['step', ['get', 'point_count'], 16, 10, 20, 50, 26],
          'circle-stroke-width': 2, 'circle-stroke-color': '#fff',
        } });
      map.addLayer({ id: 'cluster-count', type: 'symbol', source: 'sites', filter: ['has', 'point_count'],
        layout: { 'text-field': ['get', 'point_count_abbreviated'], 'text-size': 12, 'text-font': ['Noto Sans Bold'] },
        paint: { 'text-color': '#fff' } });
      map.addLayer({ id: 'site-points', type: 'circle', source: 'sites', filter: ['!', ['has', 'point_count']],
        paint: {
          'circle-color': colorExpr, 'circle-radius': 8,
          'circle-stroke-width': 2, 'circle-stroke-color': '#fff',
        } });

      map.on('click', 'clusters', async e => {
        const f = map.queryRenderedFeatures(e.point, { layers: ['clusters'] })[0];
        const zoom = await (map.getSource('sites') as GeoJSONSource).getClusterExpansionZoom(f.properties!.cluster_id);
        map.easeTo({ center: (f.geometry as Point).coordinates as [number, number], zoom });
      });

      map.on('click', 'site-points', e => {
        const f = e.features![0];
        map.flyTo({ center: (f.geometry as Point).coordinates as [number, number],
          zoom: Math.max(map.getZoom(), 15), pitch: 65, duration: 1800 });
        onSelectRef.current(f.properties!.slug);
      });

      for (const layer of ['clusters', 'site-points']) {
        map.on('mouseenter', layer, () => (map.getCanvas().style.cursor = 'pointer'));
        map.on('mouseleave', layer, () => (map.getCanvas().style.cursor = ''));
      }

      readyRef.current = true;
      (map.getSource('sites') as GeoJSONSource).setData(dataRef.current); // data may have arrived mid-load
    });

    return () => { readyRef.current = false; map.remove(); mapRef.current = null; };
  }, []);

  // push new data into the source when filters change
  useEffect(() => {
    if (!readyRef.current) return;
    (mapRef.current?.getSource('sites') as GeoJSONSource | undefined)?.setData(data ?? EMPTY);
  }, [data]);

  return <div ref={containerRef} className="h-full w-full" />;
}