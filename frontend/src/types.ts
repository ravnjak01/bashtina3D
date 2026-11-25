// src/types.ts
export type SiteCategory =
  | 'Fortress' | 'Bridge' | 'Mosque' | 'Church' | 'Monastery' | 'Necropolis'
  | 'Archaeological' | 'OldTown' | 'Museum' | 'Monument' | 'Other';

export interface ModelInfo { url: string; scale: number; headingDeg: number; altitudeOffsetM: number; fileSizeBytes: number | null }
export interface MediaInfo { url: string; caption: string | null; attribution: string | null; license: string | null }

export interface SiteDetail {
  id: string; name: string; nameLocal: string | null; slug: string; description: string | null;
  category: SiteCategory; period: string | null; yearBuilt: number | null; municipality: string | null;
  isNationalMonument: boolean; longitude: number; latitude: number; elevationM: number | null;
  wikidataId: string | null; heroImageUrl: string | null; model: ModelInfo | null; media: MediaInfo[];
}