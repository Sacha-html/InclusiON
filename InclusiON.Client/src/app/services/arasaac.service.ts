import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

export interface ArasaacPictogram {
  id: number;
  keyword: string;
  imageUrl: string;
}

interface ArasaacApiResult {
  _id: number;
  keywords: { keyword: string }[];
}

@Injectable({ providedIn: 'root' })
export class ArasaacService {
  private readonly http = inject(HttpClient);

  // Legacy activity content uses these semantic slugs instead of numeric IDs.
  // Keep this allowlist explicit: unknown text must never become an ARASAAC URL.
  private readonly legacyPictogramIds: Record<string, number> = {
    pic_taza_der: 2582,
    // Legacy cup pieces intentionally point to different pictograms so they remain distinct.
    pic_taza_izq: 2581,
    comer: 28667,
    despertar: 8989,
    jugar: 23392,
    // Roadmap content previously referenced this invalid ARASAAC ID for "jugar".
    '32449': 23392,
  };

  search(term: string): Observable<ArasaacPictogram[]> {
    const url = `https://api.arasaac.org/api/pictograms/es/search/${encodeURIComponent(term)}`;
    return this.http.get<ArasaacApiResult[]>(url).pipe(
      map((results) =>
        results.slice(0, 20).map((r) => ({
          id: r._id,
          keyword: r.keywords[0]?.keyword ?? term,
          imageUrl: this.getPictogramUrl(r._id),
        }))
      )
    );
  }

  getPictogramUrl(id: number): string;
  getPictogramUrl(id: string | null | undefined): string | null;
  getPictogramUrl(id: number | string | null | undefined): string | null;
  getPictogramUrl(id: number | string | null | undefined): string | null {
    const normalizedId = typeof id === 'number' ? String(id) : id?.trim();
    const legacyKey = normalizedId?.toLowerCase();
    const resolvedId = legacyKey && this.legacyPictogramIds[legacyKey]
      ? String(this.legacyPictogramIds[legacyKey])
      : normalizedId;

    // Reject unknown slugs instead of manufacturing URLs that return 404/HTML.
    if (!resolvedId || !/^\d+$/.test(resolvedId) || Number(resolvedId) <= 0) {
      return null;
    }

    return `https://static.arasaac.org/pictograms/${resolvedId}/${resolvedId}_500.png`;
  }
}
