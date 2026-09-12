import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ArasaacService } from './arasaac.service';

describe('ArasaacService', () => {
  let service: ArasaacService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ArasaacService, provideHttpClient()],
    });
    service = TestBed.inject(ArasaacService);
  });

  it('builds the static URL only for numeric ARASAAC identifiers', () => {
    expect(service.getPictogramUrl(9214)).toBe(
      'https://static.arasaac.org/pictograms/9214/9214_500.png'
    );
    expect(service.getPictogramUrl('9214')).toBe(
      'https://static.arasaac.org/pictograms/9214/9214_500.png'
    );
  });

  it('resolves the inherited invalid play ID to the valid ARASAAC pictogram', () => {
    const expected = 'https://static.arasaac.org/pictograms/23392/23392_500.png';

    expect(service.getPictogramUrl(32449)).toBe(expected);
    expect(service.getPictogramUrl('32449')).toBe(expected);
    expect(service.getPictogramUrl(23392)).toBe(expected);
  });

  it('resolves legacy cup slugs to distinct ARASAAC pictograms', () => {
    const rightCup = 'https://static.arasaac.org/pictograms/2582/2582_500.png';
    const leftCup = 'https://static.arasaac.org/pictograms/2581/2581_500.png';

    expect(service.getPictogramUrl('pic_taza_der')).toBe(rightCup);
    expect(service.getPictogramUrl('pic_taza_izq')).toBe(leftCup);
  });

  it('resolves inherited semantic activity aliases, including surrounding spaces and case', () => {
    expect(service.getPictogramUrl('comer')).toBe(
      'https://static.arasaac.org/pictograms/28667/28667_500.png'
    );
    expect(service.getPictogramUrl('  DESPERTAR  ')).toBe(
      'https://static.arasaac.org/pictograms/8989/8989_500.png'
    );
    expect(service.getPictogramUrl('Jugar')).toBe(
      'https://static.arasaac.org/pictograms/23392/23392_500.png'
    );
  });

  it('does not manufacture an ARASAAC URL for an unknown slug', () => {
    expect(service.getPictogramUrl('pic_taza_desconocida')).toBeNull();
    expect(service.getPictogramUrl('comer algo')).toBeNull();
    expect(service.getPictogramUrl(undefined)).toBeNull();
  });
});
