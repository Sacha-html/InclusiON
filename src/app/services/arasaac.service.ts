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

  getPictogramUrl(id: number): string {
    return `https://static.arasaac.org/pictograms/${id}/${id}_500.png`;
  }
}
