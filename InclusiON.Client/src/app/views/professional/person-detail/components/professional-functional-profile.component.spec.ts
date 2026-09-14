import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { CatalogsService, PersonsService, ToastService } from '@services';
import { ProfessionalFunctionalProfileComponent } from './professional-functional-profile.component';

describe('ProfessionalFunctionalProfileComponent text limits', () => {
  let fixture: ComponentFixture<ProfessionalFunctionalProfileComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ProfessionalFunctionalProfileComponent],
      providers: [
        { provide: CatalogsService, useValue: { getAutonomyLevels: () => of([]), getLoginMethods: () => of([]), getAvatarColors: () => of([]) } },
        { provide: PersonsService, useValue: {} },
        { provide: ToastService, useValue: { error: () => {} } },
      ],
    });
    fixture = TestBed.createComponent(ProfessionalFunctionalProfileComponent);
    fixture.componentInstance.person = { id: 'person-id' } as any;
    fixture.componentInstance.startEditing();
    fixture.detectChanges();
  });

  it('exposes the agreed maxlength values and counters for every descriptive field', () => {
    const textareas = Array.from(fixture.nativeElement.querySelectorAll('textarea')) as HTMLTextAreaElement[];

    expect(textareas.map(textarea => textarea.maxLength)).toEqual([500, 250, 255, 500]);
    expect(fixture.nativeElement.textContent).toContain('0/500 caracteres');
    expect(fixture.nativeElement.textContent).toContain('0/250 caracteres');
    expect(fixture.nativeElement.textContent).toContain('0/255 caracteres');
  });
});
